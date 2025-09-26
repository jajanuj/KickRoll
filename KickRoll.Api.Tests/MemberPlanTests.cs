using Google.Cloud.Firestore;
using KickRoll.Api.Models;

namespace KickRoll.Api.Tests;

public class MemberPlanTests
{
    [Fact]
    public void 會員方案_應包含所需屬性()
    {
        // 準備與執行
        var plan = new MemberPlan
        {
            Id = "test-plan",
            Type = "credit_pack",
            Name = "10堂課程包",
            TotalCredits = 10,
            RemainingCredits = 8,
            ValidFrom = Timestamp.GetCurrentTimestamp(),
            ValidUntil = Timestamp.FromDateTime(DateTime.UtcNow.AddMonths(3)),
            Status = "active",
            CreatedAt = Timestamp.GetCurrentTimestamp(),
            UpdatedAt = Timestamp.GetCurrentTimestamp()
        };

        // 驗證
        Assert.Equal("test-plan", plan.Id);
        Assert.Equal("credit_pack", plan.Type);
        Assert.Equal("10堂課程包", plan.Name);
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
    public void 會員方案_應支援有效的方案類型(string planType)
    {
        // 準備與執行
        var plan = new MemberPlan
        {
            Type = planType,
            Name = "測試方案",
            RemainingCredits = 5,
            Status = "active",
            CreatedAt = Timestamp.GetCurrentTimestamp(),
            UpdatedAt = Timestamp.GetCurrentTimestamp()
        };

        // 驗證
        Assert.Equal(planType, plan.Type);
    }

    [Theory]
    [InlineData("active")]
    [InlineData("expired")]
    [InlineData("suspended")]
    public void 會員方案_應支援有效的狀態值(string status)
    {
        // 準備與執行
        var plan = new MemberPlan
        {
            Type = "credit_pack",
            Name = "測試方案",
            RemainingCredits = 5,
            Status = status,
            CreatedAt = Timestamp.GetCurrentTimestamp(),
            UpdatedAt = Timestamp.GetCurrentTimestamp()
        };

        // 驗證
        Assert.Equal(status, plan.Status);
    }

    [Fact]
    public void 會員方案_預設狀態應為啟用()
    {
        // 準備與執行
        var plan = new MemberPlan();

        // 驗證
        Assert.Equal("active", plan.Status);
    }

    [Fact]
    public void 會員方案_期限票允許總堂數為空值()
    {
        // 準備與執行
        var plan = new MemberPlan
        {
            Type = "time_pass",
            Name = "月票",
            TotalCredits = null, // 期限票沒有總堂數
            RemainingCredits = 0, // 期限票不適用剩餘堂數
            ValidFrom = Timestamp.GetCurrentTimestamp(),
            ValidUntil = Timestamp.FromDateTime(DateTime.UtcNow.AddMonths(1)),
            Status = "active",
            CreatedAt = Timestamp.GetCurrentTimestamp(),
            UpdatedAt = Timestamp.GetCurrentTimestamp()
        };

        // 驗證
        Assert.Null(plan.TotalCredits);
        Assert.Equal("time_pass", plan.Type);
    }

    [Fact]
    public void 建立會員方案請求_應包含所需屬性()
    {
        // 準備與執行
        var request = new CreateMemberPlanRequest
        {
            Type = "credit_pack",
            Name = "測試方案",
            TotalCredits = 10,
            RemainingCredits = 10,
            ValidFrom = DateTime.UtcNow,
            ValidUntil = DateTime.UtcNow.AddMonths(6),
            Status = "active"
        };

        // 驗證
        Assert.Equal("credit_pack", request.Type);
        Assert.Equal("測試方案", request.Name);
        Assert.Equal(10, request.TotalCredits);
        Assert.Equal(10, request.RemainingCredits);
        Assert.NotNull(request.ValidFrom);
        Assert.NotNull(request.ValidUntil);
        Assert.Equal("active", request.Status);
    }

    [Fact]
    public void 調整堂數請求_應支援正負數差值()
    {
        // 準備與執行
        var positiveRequest = new AdjustCreditsRequest { Delta = 5, Reason = "手動增加" };
        var negativeRequest = new AdjustCreditsRequest { Delta = -2, Reason = "上課出席" };

        // 驗證
        Assert.Equal(5, positiveRequest.Delta);
        Assert.Equal("手動增加", positiveRequest.Reason);
        Assert.Equal(-2, negativeRequest.Delta);
        Assert.Equal("上課出席", negativeRequest.Reason);
    }
}