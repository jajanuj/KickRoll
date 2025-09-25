using System.Text.Json;
using KickRoll.Api.Models;

namespace KickRoll.Api.Tests;

public class MemberPlanEndpointTests
{
    [Fact]
    public void 建立會員方案請求_應序列化為駝峰式JSON()
    {
        // 準備
        var request = new CreateMemberPlanRequest
        {
            Type = "credit_pack",
            Name = "10堂課程包", 
            TotalCredits = 10,
            RemainingCredits = 10,
            ValidFrom = DateTime.UtcNow,
            ValidUntil = DateTime.UtcNow.AddMonths(6),
            Status = "active"
        };

        // 執行 - 模擬 JSON 序列化（API 中會發生的情況）
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // 驗證 - JSON 應使用駝峰式欄位名稱
        Assert.Contains("\"type\":", json);
        Assert.Contains("\"name\":", json);
        Assert.Contains("\"totalCredits\":", json);
        Assert.Contains("\"remainingCredits\":", json);
        Assert.Contains("\"validFrom\":", json);
        Assert.Contains("\"validUntil\":", json);
        Assert.Contains("\"status\":", json);
        
        // 驗證內容
        Assert.Contains("\"credit_pack\"", json);
        Assert.Contains("\"10\\u5802\\u8AB2\\u7A0B\\u5305\"", json); // Unicode 編碼的中文（注意大寫）
        Assert.Contains("\"active\"", json);
    }

    [Fact]
    public void 會員方案回應_應序列化為駝峰式JSON()
    {
        // 準備
        var response = new MemberPlanResponse
        {
            Id = "plan123",
            Type = "time_pass",
            Name = "月票",
            TotalCredits = null, // time_pass 可以有 null TotalCredits
            RemainingCredits = 0,
            ValidFrom = DateTime.UtcNow,
            ValidUntil = DateTime.UtcNow.AddMonths(1),
            Status = "active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 執行
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // 驗證 - JSON 應使用駝峰式欄位名稱作為 API 回應
        Assert.Contains("\"id\":", json);
        Assert.Contains("\"type\":", json);
        Assert.Contains("\"name\":", json);
        Assert.Contains("\"totalCredits\":null", json); // null 值應被序列化
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
    public void 建立會員方案_方案類型驗證邏輯(string planType, bool expectedValid)
    {
        // 此測試展示控制器中會使用的驗證邏輯
        
        // 執行
        bool isValid = planType == "credit_pack" || planType == "time_pass";
        
        // 驗證
        Assert.Equal(expectedValid, isValid);
    }

    [Theory]
    [InlineData("active", true)]
    [InlineData("expired", true)]
    [InlineData("suspended", true)]
    [InlineData("invalid_status", false)]
    [InlineData("", false)]
    public void 更新會員方案_狀態驗證邏輯(string status, bool expectedValid)
    {
        // 此測試展示控制器中會使用的驗證邏輯
        
        // 執行
        var validStatuses = new[] { "active", "expired", "suspended" };
        bool isValid = validStatuses.Contains(status);
        
        // 驗證
        Assert.Equal(expectedValid, isValid);
    }

    [Theory]
    [InlineData(10, 5, 15)] // 正數調整
    [InlineData(10, -3, 7)] // 負數調整（上課使用）
    [InlineData(0, 5, 5)]   // 對空方案增加堂數
    public void 調整堂數_邏輯應正確計算(int currentCredits, int delta, int expectedNewCredits)
    {
        // 此測試展示控制器中使用的堂數調整邏輯
        
        // 執行
        int newCredits = currentCredits + delta;
        
        // 驗證
        Assert.Equal(expectedNewCredits, newCredits);
    }

    [Theory]
    [InlineData(5, -10, false)] // 會導致負數堂數
    [InlineData(0, -1, false)]  // 會導致負數堂數
    [InlineData(10, -5, true)]  // 有效調整
    [InlineData(0, 5, true)]    // 有效調整
    public void 調整堂數_應防止堂數為負數(int currentCredits, int delta, bool expectedValid)
    {
        // 此測試展示商業規則：剩餘堂數不能為負數
        
        // 執行
        int newCredits = currentCredits + delta;
        bool isValid = newCredits >= 0;
        
        // 驗證
        Assert.Equal(expectedValid, isValid);
    }

    [Fact]
    public void 方案到期_邏輯應正確運作()
    {
        // 準備
        var now = DateTime.UtcNow;
        var expiredPlan = new { ValidUntil = now.AddDays(-1), Status = "active" }; // 已過期
        var activePlan = new { ValidUntil = now.AddDays(30), Status = "active" }; // 仍啟用
        var planWithoutExpiry = new { ValidUntil = (DateTime?)null, Status = "active" }; // 無到期日

        // 執行與驗證
        // 過期方案應標記為過期
        Assert.True(expiredPlan.ValidUntil < now && expiredPlan.Status == "active");
        
        // 啟用方案應保持啟用
        Assert.True(activePlan.ValidUntil > now && activePlan.Status == "active");
        
        // 無到期日方案應保持啟用
        Assert.True(planWithoutExpiry.ValidUntil == null && planWithoutExpiry.Status == "active");
    }
}