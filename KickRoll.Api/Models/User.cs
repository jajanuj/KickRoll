using Google.Cloud.Firestore;

namespace KickRoll.Api.Models;

[FirestoreData]
public class User
{
    [FirestoreDocumentId]
    public string UserId { get; set; } = string.Empty;

    [FirestoreProperty(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [FirestoreProperty(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [FirestoreProperty(Name = "Role")]
    public string Role { get; set; } = "Member"; // Member|Coach|Admin

    [FirestoreProperty(Name = "Status")]
    public string Status { get; set; } = "PendingVerify"; // Active|Disabled|PendingVerify

    [FirestoreProperty(Name = "EmailVerified")]
    public bool EmailVerified { get; set; } = false;

    [FirestoreProperty(Name = "CreatedAt")]
    public Timestamp CreatedAt { get; set; }

    [FirestoreProperty(Name = "UpdatedAt")]
    public Timestamp UpdatedAt { get; set; }
}

public class UserRegistrationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool AcceptTerms { get; set; } = false;
}

public class UserLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class PasswordResetRequest
{
    public string Email { get; set; } = string.Empty;
}

public class UpdateUserRoleRequest
{
    public string Role { get; set; } = string.Empty; // Member|Coach|Admin
}

public class UpdateUserStatusRequest
{
    public string Status { get; set; } = string.Empty; // Active|Disabled|PendingVerify
}

public class UserResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}