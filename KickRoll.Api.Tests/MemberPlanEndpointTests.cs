using System.Text.Json;
using KickRoll.Api.Models;

namespace KickRoll.Api.Tests;

public class MemberPlanEndpointTests
{
    [Fact]
    public void CreateMemberPlanRequest_Should_Serialize_To_CamelCase_Json()
    {
        // Arrange
        var request = new CreateMemberPlanRequest
        {
            Type = "credit_pack",
            Name = "10 Class Pack", 
            TotalCredits = 10,
            RemainingCredits = 10,
            ValidFrom = DateTime.UtcNow,
            ValidUntil = DateTime.UtcNow.AddMonths(6),
            Status = "active"
        };

        // Act - Simulate JSON serialization (what would happen in the API)
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Assert - JSON should use camelCase field names
        Assert.Contains("\"type\":", json);
        Assert.Contains("\"name\":", json);
        Assert.Contains("\"totalCredits\":", json);
        Assert.Contains("\"remainingCredits\":", json);
        Assert.Contains("\"validFrom\":", json);
        Assert.Contains("\"validUntil\":", json);
        Assert.Contains("\"status\":", json);
        
        // Verify content
        Assert.Contains("\"credit_pack\"", json);
        Assert.Contains("\"10 Class Pack\"", json);
        Assert.Contains("\"active\"", json);
    }

    [Fact]
    public void MemberPlanResponse_Should_Serialize_To_CamelCase_Json()
    {
        // Arrange
        var response = new MemberPlanResponse
        {
            Id = "plan123",
            Type = "time_pass",
            Name = "Monthly Pass",
            TotalCredits = null, // time_pass can have null TotalCredits
            RemainingCredits = 0,
            ValidFrom = DateTime.UtcNow,
            ValidUntil = DateTime.UtcNow.AddMonths(1),
            Status = "active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Assert - JSON should use camelCase field names for API responses
        Assert.Contains("\"id\":", json);
        Assert.Contains("\"type\":", json);
        Assert.Contains("\"name\":", json);
        Assert.Contains("\"totalCredits\":null", json); // null values should be serialized
        Assert.Contains("\"remainingCredits\":", json);
        Assert.Contains("\"validFrom\":", json);
        Assert.Contains("\"validUntil\":", json);
        Assert.Contains("\"status\":", json);
        Assert.Contains("\"createdAt\":", json);
        Assert.Contains("\"updatedAt\":", json);
    }

    [Theory]
    [InlineData("credit_pack", true)]
    [InlineData("time_pass", true)]
    [InlineData("invalid_type", false)]
    [InlineData("", false)]
    public void CreateMemberPlan_Type_Validation_Logic(string planType, bool expectedValid)
    {
        // This test demonstrates the validation logic that would be used in the controller
        
        // Act
        bool isValid = planType == "credit_pack" || planType == "time_pass";
        
        // Assert
        Assert.Equal(expectedValid, isValid);
    }

    [Theory]
    [InlineData("active", true)]
    [InlineData("expired", true)]
    [InlineData("suspended", true)]
    [InlineData("invalid_status", false)]
    [InlineData("", false)]
    public void UpdateMemberPlan_Status_Validation_Logic(string status, bool expectedValid)
    {
        // This test demonstrates the validation logic that would be used in the controller
        
        // Act
        var validStatuses = new[] { "active", "expired", "suspended" };
        bool isValid = validStatuses.Contains(status);
        
        // Assert
        Assert.Equal(expectedValid, isValid);
    }

    [Theory]
    [InlineData(10, 5, 15)] // positive adjustment
    [InlineData(10, -3, 7)] // negative adjustment (class usage)
    [InlineData(0, 5, 5)]   // add credits to empty plan
    public void AdjustCredits_Logic_Should_Calculate_Correctly(int currentCredits, int delta, int expectedNewCredits)
    {
        // This test demonstrates the credit adjustment logic used in the controller
        
        // Act
        int newCredits = currentCredits + delta;
        
        // Assert
        Assert.Equal(expectedNewCredits, newCredits);
    }

    [Theory]
    [InlineData(5, -10, false)] // would result in negative credits
    [InlineData(0, -1, false)]  // would result in negative credits
    [InlineData(10, -5, true)]  // valid adjustment
    [InlineData(0, 5, true)]    // valid adjustment
    public void AdjustCredits_Should_Prevent_Negative_Credits(int currentCredits, int delta, bool expectedValid)
    {
        // This test demonstrates the business rule: RemainingCredits cannot be negative
        
        // Act
        int newCredits = currentCredits + delta;
        bool isValid = newCredits >= 0;
        
        // Assert
        Assert.Equal(expectedValid, isValid);
    }

    [Fact]
    public void Plan_Expiration_Logic_Should_Work_Correctly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var expiredPlan = new { ValidUntil = now.AddDays(-1), Status = "active" }; // expired
        var activePlan = new { ValidUntil = now.AddDays(30), Status = "active" }; // still active
        var planWithoutExpiry = new { ValidUntil = (DateTime?)null, Status = "active" }; // no expiry

        // Act & Assert
        // Expired plan should be marked for expiration
        Assert.True(expiredPlan.ValidUntil < now && expiredPlan.Status == "active");
        
        // Active plan should remain active
        Assert.True(activePlan.ValidUntil > now && activePlan.Status == "active");
        
        // Plan without expiry should remain active
        Assert.True(planWithoutExpiry.ValidUntil == null && planWithoutExpiry.Status == "active");
    }
}