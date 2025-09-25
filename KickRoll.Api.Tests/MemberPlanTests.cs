using Google.Cloud.Firestore;
using KickRoll.Api.Models;

namespace KickRoll.Api.Tests;

public class MemberPlanTests
{
    [Fact]
    public void MemberPlan_Should_Have_Required_Properties()
    {
        // Arrange & Act
        var plan = new MemberPlan
        {
            Id = "test-plan",
            Type = "credit_pack",
            Name = "10 Class Pack",
            TotalCredits = 10,
            RemainingCredits = 8,
            ValidFrom = Timestamp.GetCurrentTimestamp(),
            ValidUntil = Timestamp.FromDateTime(DateTime.UtcNow.AddMonths(3)),
            Status = "active",
            CreatedAt = Timestamp.GetCurrentTimestamp(),
            UpdatedAt = Timestamp.GetCurrentTimestamp()
        };

        // Assert
        Assert.Equal("test-plan", plan.Id);
        Assert.Equal("credit_pack", plan.Type);
        Assert.Equal("10 Class Pack", plan.Name);
        Assert.Equal(10, plan.TotalCredits);
        Assert.Equal(8, plan.RemainingCredits);
        Assert.NotNull(plan.ValidFrom);
        Assert.NotNull(plan.ValidUntil);
        Assert.Equal("active", plan.Status);
        Assert.True(plan.CreatedAt.ToDateTime() <= DateTime.UtcNow);
        Assert.True(plan.UpdatedAt.ToDateTime() <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData("credit_pack")]
    [InlineData("time_pass")]
    public void MemberPlan_Should_Support_Valid_Types(string planType)
    {
        // Arrange & Act
        var plan = new MemberPlan
        {
            Type = planType,
            Name = "Test Plan",
            RemainingCredits = 5,
            Status = "active",
            CreatedAt = Timestamp.GetCurrentTimestamp(),
            UpdatedAt = Timestamp.GetCurrentTimestamp()
        };

        // Assert
        Assert.Equal(planType, plan.Type);
    }

    [Theory]
    [InlineData("active")]
    [InlineData("expired")]
    [InlineData("suspended")]
    public void MemberPlan_Should_Support_Valid_Statuses(string status)
    {
        // Arrange & Act
        var plan = new MemberPlan
        {
            Type = "credit_pack",
            Name = "Test Plan",
            RemainingCredits = 5,
            Status = status,
            CreatedAt = Timestamp.GetCurrentTimestamp(),
            UpdatedAt = Timestamp.GetCurrentTimestamp()
        };

        // Assert
        Assert.Equal(status, plan.Status);
    }

    [Fact]
    public void MemberPlan_Default_Status_Should_Be_Active()
    {
        // Arrange & Act
        var plan = new MemberPlan();

        // Assert
        Assert.Equal("active", plan.Status);
    }

    [Fact]
    public void MemberPlan_Should_Allow_Null_TotalCredits_For_TimePasses()
    {
        // Arrange & Act
        var plan = new MemberPlan
        {
            Type = "time_pass",
            Name = "Monthly Pass",
            TotalCredits = null, // Time passes don't have total credits
            RemainingCredits = 0, // Not applicable for time passes
            ValidFrom = Timestamp.GetCurrentTimestamp(),
            ValidUntil = Timestamp.FromDateTime(DateTime.UtcNow.AddMonths(1)),
            Status = "active",
            CreatedAt = Timestamp.GetCurrentTimestamp(),
            UpdatedAt = Timestamp.GetCurrentTimestamp()
        };

        // Assert
        Assert.Null(plan.TotalCredits);
        Assert.Equal("time_pass", plan.Type);
    }

    [Fact]
    public void CreateMemberPlanRequest_Should_Have_Required_Properties()
    {
        // Arrange & Act
        var request = new CreateMemberPlanRequest
        {
            Type = "credit_pack",
            Name = "Test Plan",
            TotalCredits = 10,
            RemainingCredits = 10,
            ValidFrom = DateTime.UtcNow,
            ValidUntil = DateTime.UtcNow.AddMonths(6),
            Status = "active"
        };

        // Assert
        Assert.Equal("credit_pack", request.Type);
        Assert.Equal("Test Plan", request.Name);
        Assert.Equal(10, request.TotalCredits);
        Assert.Equal(10, request.RemainingCredits);
        Assert.NotNull(request.ValidFrom);
        Assert.NotNull(request.ValidUntil);
        Assert.Equal("active", request.Status);
    }

    [Fact]
    public void AdjustCreditsRequest_Should_Support_Positive_And_Negative_Delta()
    {
        // Arrange & Act
        var positiveRequest = new AdjustCreditsRequest { Delta = 5, Reason = "Manual add" };
        var negativeRequest = new AdjustCreditsRequest { Delta = -2, Reason = "Class attended" };

        // Assert
        Assert.Equal(5, positiveRequest.Delta);
        Assert.Equal("Manual add", positiveRequest.Reason);
        Assert.Equal(-2, negativeRequest.Delta);
        Assert.Equal("Class attended", negativeRequest.Reason);
    }
}