using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using KickRoll.Api.Models;

namespace KickRoll.Api.Services;

public interface IAuthService
{
    Task<(bool Success, string Message, UserResponse? User)> RegisterUserAsync(UserRegistrationRequest request, string? ip = null);
    Task<(bool Success, string Message, UserResponse? User, string? Token)> LoginUserAsync(UserLoginRequest request, string? ip = null);
    Task<(bool Success, string Message)> SendEmailVerificationAsync(string userId);
    Task<(bool Success, string Message)> SendPasswordResetAsync(string email);
    Task<(bool Success, string Message)> VerifyEmailAsync(string userId);
    Task<(bool Success, string Message, UserResponse?)> GetUserByIdAsync(string userId);
    Task<UserResponse?> GetCurrentUserAsync(string userId);
    Task<bool> IsUserActiveAndVerifiedAsync(string userId);
}

public class AuthService : IAuthService
{
    private readonly FirebaseAuth _firebaseAuth;
    private readonly FirestoreDb _firestore;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(FirebaseAuth firebaseAuth, FirestoreDb firestore, IAuditService auditService, ILogger<AuthService> logger)
    {
        _firebaseAuth = firebaseAuth;
        _firestore = firestore;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, UserResponse? User)> RegisterUserAsync(UserRegistrationRequest request, string? ip = null)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Name))
                return (false, "姓名不可空白", null);

            if (string.IsNullOrWhiteSpace(request.Email))
                return (false, "電子郵件不可空白", null);

            if (string.IsNullOrWhiteSpace(request.Password))
                return (false, "密碼不可空白", null);

            if (!request.AcceptTerms)
                return (false, "必須同意服務條款", null);

            if (request.Password.Length < 6)
                return (false, "密碼長度至少需要6個字元", null);

            // Check if email already exists
            try
            {
                await _firebaseAuth.GetUserByEmailAsync(request.Email);
                return (false, "此電子郵件已被註冊", null);
            }
            catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
            {
                // User doesn't exist, continue with registration
            }

            // Create Firebase user
            var userArgs = new UserRecordArgs
            {
                Email = request.Email,
                Password = request.Password,
                DisplayName = request.Name,
                EmailVerified = false,
                Disabled = false
            };

            var firebaseUser = await _firebaseAuth.CreateUserAsync(userArgs);

            // Create user document in Firestore
            var now = Timestamp.GetCurrentTimestamp();
            var user = new User
            {
                UserId = firebaseUser.Uid,
                Name = request.Name,
                Email = request.Email,
                Role = "Member",
                Status = "PendingVerify",
                EmailVerified = false,
                CreatedAt = now,
                UpdatedAt = now
            };

            var userRef = _firestore.Collection("users").Document(firebaseUser.Uid);
            await userRef.SetAsync(user);

            // Send email verification
            var link = await _firebaseAuth.GenerateEmailVerificationLinkAsync(request.Email);
            _logger.LogInformation($"Email verification link generated for {request.Email}: {link}");

            // Audit log
            await _auditService.LogAsync("USER_REGISTER", "User", firebaseUser.Uid, firebaseUser.Uid, 
                new Dictionary<string, object> { ["email"] = request.Email, ["name"] = request.Name }, ip);

            var userResponse = new UserResponse
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                Status = user.Status,
                EmailVerified = user.EmailVerified,
                CreatedAt = user.CreatedAt.ToDateTime(),
                UpdatedAt = user.UpdatedAt.ToDateTime()
            };

            return (true, "註冊成功，請至電子郵件信箱完成驗證", userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return (false, $"註冊失敗：{ex.Message}", null);
        }
    }

    public async Task<(bool Success, string Message, UserResponse? User, string? Token)> LoginUserAsync(UserLoginRequest request, string? ip = null)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email))
                return (false, "電子郵件不可空白", null, null);

            if (string.IsNullOrWhiteSpace(request.Password))
                return (false, "密碼不可空白", null, null);

            // Get Firebase user by email
            UserRecord firebaseUser;
            try
            {
                firebaseUser = await _firebaseAuth.GetUserByEmailAsync(request.Email);
            }
            catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
            {
                return (false, "電子郵件或密碼錯誤", null, null);
            }

            // Get user from Firestore
            var userRef = _firestore.Collection("users").Document(firebaseUser.Uid);
            var userSnapshot = await userRef.GetSnapshotAsync();

            if (!userSnapshot.Exists)
            {
                return (false, "使用者不存在", null, null);
            }

            var user = userSnapshot.ConvertTo<User>();

            // Check if user is active and verified
            if (user.Status != "Active")
            {
                if (user.Status == "PendingVerify")
                    return (false, "請先完成電子郵件驗證", null, null);
                else if (user.Status == "Disabled")
                    return (false, "此帳號已被停用，請聯絡管理員", null, null);
            }

            if (!user.EmailVerified)
            {
                return (false, "請先完成電子郵件驗證", null, null);
            }

            // Generate custom token for the user
            var customToken = await _firebaseAuth.CreateCustomTokenAsync(firebaseUser.Uid, new Dictionary<string, object>
            {
                ["role"] = user.Role,
                ["email"] = user.Email,
                ["emailVerified"] = user.EmailVerified,
                ["status"] = user.Status
            });

            // Audit log
            await _auditService.LogAsync("USER_LOGIN", "User", firebaseUser.Uid, firebaseUser.Uid, 
                new Dictionary<string, object> { ["email"] = request.Email }, ip);

            var userResponse = new UserResponse
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                Status = user.Status,
                EmailVerified = user.EmailVerified,
                CreatedAt = user.CreatedAt.ToDateTime(),
                UpdatedAt = user.UpdatedAt.ToDateTime()
            };

            return (true, "登入成功", userResponse, customToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login");
            return (false, $"登入失敗：{ex.Message}", null, null);
        }
    }

    public async Task<(bool Success, string Message)> SendEmailVerificationAsync(string userId)
    {
        try
        {
            var firebaseUser = await _firebaseAuth.GetUserAsync(userId);
            var link = await _firebaseAuth.GenerateEmailVerificationLinkAsync(firebaseUser.Email);
            _logger.LogInformation($"Email verification link generated for {firebaseUser.Email}: {link}");

            return (true, "驗證信已重新寄送");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email verification");
            return (false, $"驗證信寄送失敗：{ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> SendPasswordResetAsync(string email)
    {
        try
        {
            var link = await _firebaseAuth.GeneratePasswordResetLinkAsync(email);
            _logger.LogInformation($"Password reset link generated for {email}: {link}");

            return (true, "密碼重設信已寄送");
        }
        catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
        {
            // Don't reveal that user doesn't exist
            return (true, "若電子郵件存在，密碼重設信已寄送");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email");
            return (false, $"密碼重設信寄送失敗：{ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> VerifyEmailAsync(string userId)
    {
        try
        {
            // Update Firebase user
            var userUpdateArgs = new UserRecordArgs
            {
                Uid = userId,
                EmailVerified = true
            };
            await _firebaseAuth.UpdateUserAsync(userUpdateArgs);

            // Update Firestore user document
            var userRef = _firestore.Collection("users").Document(userId);
            var updates = new Dictionary<string, object>
            {
                ["EmailVerified"] = true,
                ["Status"] = "Active", // Automatically activate upon email verification
                ["UpdatedAt"] = Timestamp.GetCurrentTimestamp()
            };

            await userRef.SetAsync(updates, SetOptions.MergeAll);

            // Audit log
            await _auditService.LogAsync("EMAIL_VERIFIED", "User", userId, userId, null, null);

            return (true, "電子郵件驗證完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email");
            return (false, $"電子郵件驗證失敗：{ex.Message}");
        }
    }

    public async Task<(bool Success, string Message, UserResponse?)> GetUserByIdAsync(string userId)
    {
        try
        {
            var userRef = _firestore.Collection("users").Document(userId);
            var userSnapshot = await userRef.GetSnapshotAsync();

            if (!userSnapshot.Exists)
            {
                return (false, "使用者不存在", null);
            }

            var user = userSnapshot.ConvertTo<User>();
            var userResponse = new UserResponse
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                Status = user.Status,
                EmailVerified = user.EmailVerified,
                CreatedAt = user.CreatedAt.ToDateTime(),
                UpdatedAt = user.UpdatedAt.ToDateTime()
            };

            return (true, "使用者資料取得成功", userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID");
            return (false, $"取得使用者資料失敗：{ex.Message}", null);
        }
    }

    public async Task<UserResponse?> GetCurrentUserAsync(string userId)
    {
        try
        {
            var (success, _, user) = await GetUserByIdAsync(userId);
            return success ? user : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsUserActiveAndVerifiedAsync(string userId)
    {
        try
        {
            var user = await GetCurrentUserAsync(userId);
            return user?.Status == "Active" && user.EmailVerified;
        }
        catch
        {
            return false;
        }
    }
}