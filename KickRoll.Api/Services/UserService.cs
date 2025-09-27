using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using KickRoll.Api.Models;

namespace KickRoll.Api.Services;

public interface IUserService
{
    Task<List<UserResponse>> GetUsersAsync(string? searchTerm = null, string? role = null, string? status = null, int limit = 100);
    Task<(bool Success, string Message)> UpdateUserRoleAsync(string userId, string newRole, string actorUserId, string? ip = null);
    Task<(bool Success, string Message)> UpdateUserStatusAsync(string userId, string newStatus, string actorUserId, string? ip = null);
    Task<(bool Success, string Message)> CreateInitialAdminAsync(string email, string password, string name);
}

public class UserService : IUserService
{
    private readonly FirebaseAuth _firebaseAuth;
    private readonly FirestoreDb _firestore;
    private readonly IAuditService _auditService;
    private readonly ILogger<UserService> _logger;

    public UserService(FirebaseAuth firebaseAuth, FirestoreDb firestore, IAuditService auditService, ILogger<UserService> logger)
    {
        _firebaseAuth = firebaseAuth;
        _firestore = firestore;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<List<UserResponse>> GetUsersAsync(string? searchTerm = null, string? role = null, string? status = null, int limit = 100)
    {
        try
        {
            Query query = _firestore.Collection("users");

            if (!string.IsNullOrEmpty(role))
                query = query.WhereEqualTo("Role", role);

            if (!string.IsNullOrEmpty(status))
                query = query.WhereEqualTo("Status", status);

            query = query.OrderBy("CreatedAt").Limit(limit);

            var snapshot = await query.GetSnapshotAsync();
            var users = new List<UserResponse>();

            foreach (var doc in snapshot.Documents)
            {
                var user = doc.ConvertTo<User>();
                
                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    if (!user.Name.ToLower().Contains(searchLower) && 
                        !user.Email.ToLower().Contains(searchLower))
                        continue;
                }

                users.Add(new UserResponse
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    Status = user.Status,
                    EmailVerified = user.EmailVerified,
                    CreatedAt = user.CreatedAt.ToDateTime(),
                    UpdatedAt = user.UpdatedAt.ToDateTime()
                });
            }

            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get users");
            return new List<UserResponse>();
        }
    }

    public async Task<(bool Success, string Message)> UpdateUserRoleAsync(string userId, string newRole, string actorUserId, string? ip = null)
    {
        try
        {
            // Validate role
            var validRoles = new[] { "Member", "Coach", "Admin" };
            if (!validRoles.Contains(newRole))
                return (false, "角色必須是 Member、Coach 或 Admin");

            // Get current user data
            var userRef = _firestore.Collection("users").Document(userId);
            var userSnapshot = await userRef.GetSnapshotAsync();

            if (!userSnapshot.Exists)
                return (false, "使用者不存在");

            var currentUser = userSnapshot.ConvertTo<User>();
            var oldRole = currentUser.Role;

            if (oldRole == newRole)
                return (false, "使用者角色已是此設定");

            // Update Firestore document
            var updates = new Dictionary<string, object>
            {
                ["Role"] = newRole,
                ["UpdatedAt"] = Timestamp.GetCurrentTimestamp()
            };
            await userRef.SetAsync(updates, SetOptions.MergeAll);

            // Update Firebase Auth custom claims
            var customClaims = new Dictionary<string, object>
            {
                ["role"] = newRole,
                ["email"] = currentUser.Email,
                ["emailVerified"] = currentUser.EmailVerified,
                ["status"] = currentUser.Status
            };
            await _firebaseAuth.SetCustomUserClaimsAsync(userId, customClaims);

            // Audit log
            await _auditService.LogAsync("ROLE_CHANGED", "User", userId, actorUserId,
                new Dictionary<string, object>
                {
                    ["oldRole"] = oldRole,
                    ["newRole"] = newRole,
                    ["targetUserEmail"] = currentUser.Email
                }, ip);

            return (true, $"使用者角色已變更為 {newRole}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to update user role for {userId}");
            return (false, $"角色變更失敗：{ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> UpdateUserStatusAsync(string userId, string newStatus, string actorUserId, string? ip = null)
    {
        try
        {
            // Validate status
            var validStatuses = new[] { "Active", "Disabled", "PendingVerify" };
            if (!validStatuses.Contains(newStatus))
                return (false, "狀態必須是 Active、Disabled 或 PendingVerify");

            // Get current user data
            var userRef = _firestore.Collection("users").Document(userId);
            var userSnapshot = await userRef.GetSnapshotAsync();

            if (!userSnapshot.Exists)
                return (false, "使用者不存在");

            var currentUser = userSnapshot.ConvertTo<User>();
            var oldStatus = currentUser.Status;

            if (oldStatus == newStatus)
                return (false, "使用者狀態已是此設定");

            // Update Firestore document
            var updates = new Dictionary<string, object>
            {
                ["Status"] = newStatus,
                ["UpdatedAt"] = Timestamp.GetCurrentTimestamp()
            };
            await userRef.SetAsync(updates, SetOptions.MergeAll);

            // Update Firebase Auth user status
            var userUpdateArgs = new UserRecordArgs
            {
                Uid = userId,
                Disabled = newStatus == "Disabled"
            };
            await _firebaseAuth.UpdateUserAsync(userUpdateArgs);

            // Update Firebase Auth custom claims
            var customClaims = new Dictionary<string, object>
            {
                ["role"] = currentUser.Role,
                ["email"] = currentUser.Email,
                ["emailVerified"] = currentUser.EmailVerified,
                ["status"] = newStatus
            };
            await _firebaseAuth.SetCustomUserClaimsAsync(userId, customClaims);

            // Audit log
            await _auditService.LogAsync("STATUS_CHANGED", "User", userId, actorUserId,
                new Dictionary<string, object>
                {
                    ["oldStatus"] = oldStatus,
                    ["newStatus"] = newStatus,
                    ["targetUserEmail"] = currentUser.Email
                }, ip);

            return (true, $"使用者狀態已變更為 {newStatus}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to update user status for {userId}");
            return (false, $"狀態變更失敗：{ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> CreateInitialAdminAsync(string email, string password, string name)
    {
        try
        {
            // Check if any admin already exists
            var adminQuery = _firestore.Collection("users").WhereEqualTo("Role", "Admin");
            var adminSnapshot = await adminQuery.GetSnapshotAsync();

            if (adminSnapshot.Documents.Count > 0)
                return (false, "管理員帳號已存在");

            // Create Firebase user
            var userArgs = new UserRecordArgs
            {
                Email = email,
                Password = password,
                DisplayName = name,
                EmailVerified = true, // Auto-verify admin
                Disabled = false
            };

            var firebaseUser = await _firebaseAuth.CreateUserAsync(userArgs);

            // Create user document in Firestore
            var now = Timestamp.GetCurrentTimestamp();
            var user = new User
            {
                UserId = firebaseUser.Uid,
                Name = name,
                Email = email,
                Role = "Admin",
                Status = "Active", // Admin is immediately active
                EmailVerified = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            var userRef = _firestore.Collection("users").Document(firebaseUser.Uid);
            await userRef.SetAsync(user);

            // Set Firebase custom claims
            var customClaims = new Dictionary<string, object>
            {
                ["role"] = "Admin",
                ["email"] = email,
                ["emailVerified"] = true,
                ["status"] = "Active"
            };
            await _firebaseAuth.SetCustomUserClaimsAsync(firebaseUser.Uid, customClaims);

            // Audit log
            await _auditService.LogAsync("INITIAL_ADMIN_CREATED", "User", firebaseUser.Uid, firebaseUser.Uid,
                new Dictionary<string, object>
                {
                    ["email"] = email,
                    ["name"] = name
                }, null);

            _logger.LogInformation($"Initial admin created: {email}");
            return (true, "初始管理員帳號創建成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create initial admin");
            return (false, $"創建管理員失敗：{ex.Message}");
        }
    }
}