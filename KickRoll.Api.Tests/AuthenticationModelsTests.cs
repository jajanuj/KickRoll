using KickRoll.Api.Models;
using Xunit;

namespace KickRoll.Api.Tests;

public class AuthenticationModelsTests
{
    [Fact]
    public void User_Should_Have_Correct_Default_Values()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        Assert.Equal(string.Empty, user.UserId);
        Assert.Equal(string.Empty, user.Name);
        Assert.Equal(string.Empty, user.Email);
        Assert.Equal("Member", user.Role);
        Assert.Equal("PendingVerify", user.Status);
        Assert.False(user.EmailVerified);
    }

    [Fact]
    public void UserRegistrationRequest_Should_Accept_Valid_Data()
    {
        // Arrange
        var request = new UserRegistrationRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "password123",
            AcceptTerms = true
        };

        // Assert
        Assert.Equal("Test User", request.Name);
        Assert.Equal("test@example.com", request.Email);
        Assert.Equal("password123", request.Password);
        Assert.True(request.AcceptTerms);
    }

    [Fact]
    public void UserLoginRequest_Should_Accept_Valid_Credentials()
    {
        // Arrange
        var request = new UserLoginRequest
        {
            Email = "user@example.com",
            Password = "password123"
        };

        // Assert
        Assert.Equal("user@example.com", request.Email);
        Assert.Equal("password123", request.Password);
    }

    [Fact]
    public void UpdateUserRoleRequest_Should_Accept_Valid_Roles()
    {
        // Arrange & Act
        var adminRequest = new UpdateUserRoleRequest { Role = "Admin" };
        var coachRequest = new UpdateUserRoleRequest { Role = "Coach" };
        var memberRequest = new UpdateUserRoleRequest { Role = "Member" };

        // Assert
        Assert.Equal("Admin", adminRequest.Role);
        Assert.Equal("Coach", coachRequest.Role);
        Assert.Equal("Member", memberRequest.Role);
    }

    [Fact]
    public void UpdateUserStatusRequest_Should_Accept_Valid_Statuses()
    {
        // Arrange & Act
        var activeRequest = new UpdateUserStatusRequest { Status = "Active" };
        var disabledRequest = new UpdateUserStatusRequest { Status = "Disabled" };
        var pendingRequest = new UpdateUserStatusRequest { Status = "PendingVerify" };

        // Assert
        Assert.Equal("Active", activeRequest.Status);
        Assert.Equal("Disabled", disabledRequest.Status);
        Assert.Equal("PendingVerify", pendingRequest.Status);
    }

    [Fact]
    public void AuditLog_Should_Have_Correct_Default_Values()
    {
        // Arrange & Act
        var auditLog = new AuditLog();

        // Assert
        Assert.Equal(string.Empty, auditLog.LogId);
        Assert.Equal(string.Empty, auditLog.ActorUserId);
        Assert.Equal(string.Empty, auditLog.Action);
        Assert.Equal(string.Empty, auditLog.TargetType);
        Assert.Equal(string.Empty, auditLog.TargetId);
        Assert.Null(auditLog.Payload);
        Assert.Null(auditLog.Ip);
    }

    [Fact]
    public void AuditLog_Should_Accept_Payload_Data()
    {
        // Arrange
        var payload = new Dictionary<string, object>
        {
            ["oldRole"] = "Member",
            ["newRole"] = "Coach",
            ["email"] = "user@example.com"
        };

        var auditLog = new AuditLog
        {
            LogId = "log-123",
            ActorUserId = "admin-456",
            Action = "ROLE_CHANGED",
            TargetType = "User",
            TargetId = "user-789",
            Payload = payload,
            Ip = "192.168.1.1"
        };

        // Assert
        Assert.Equal("log-123", auditLog.LogId);
        Assert.Equal("admin-456", auditLog.ActorUserId);
        Assert.Equal("ROLE_CHANGED", auditLog.Action);
        Assert.Equal("User", auditLog.TargetType);
        Assert.Equal("user-789", auditLog.TargetId);
        Assert.NotNull(auditLog.Payload);
        Assert.Equal("Member", auditLog.Payload["oldRole"]);
        Assert.Equal("Coach", auditLog.Payload["newRole"]);
        Assert.Equal("192.168.1.1", auditLog.Ip);
    }
}