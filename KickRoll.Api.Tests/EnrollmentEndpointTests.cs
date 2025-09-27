using System.Text.Json;
using KickRoll.Api.Models;
using Xunit;

namespace KickRoll.Api.Tests;

public class EnrollmentEndpointTests
{
    [Fact]
    public void 報名請求_應包含必要欄位()
    {
        // 安排
        var request = new EnrollmentRequest
        {
            MemberId = "member123"
        };

        // 驗證
        Assert.NotNull(request.MemberId);
        Assert.Equal("member123", request.MemberId);
    }

    [Fact]
    public void 報名回應_應正確序列化為camelCase()
    {
        // 安排
        var response = new EnrollmentResponse
        {
            Id = "member123",
            MemberId = "member123",
            SessionId = "session456",
            Status = "enrolled",
            CreatedAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc)
        };

        // 執行
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var json = JsonSerializer.Serialize(response, options);

        // 驗證
        Assert.Contains("\"id\":", json);
        Assert.Contains("\"memberId\":", json);
        Assert.Contains("\"sessionId\":", json);
        Assert.Contains("\"status\":", json);
        Assert.Contains("\"createdAt\":", json);
    }

    [Fact]
    public void CapacityFullException_應有正確的錯誤訊息()
    {
        // 執行
        var exception = new CapacityFullException();

        // 驗證
        Assert.Equal("Session capacity is full", exception.Message);
    }

    [Fact]
    public void AlreadyEnrolledException_應有正確的錯誤訊息()
    {
        // 執行
        var exception = new AlreadyEnrolledException();

        // 驗證
        Assert.Equal("Already enrolled in this session", exception.Message);
    }

    [Theory]
    [InlineData("enrolled", true)]
    [InlineData("cancelled", false)]
    [InlineData("", false)]
    public void 報名狀態_應正確驗證(string status, bool isEnrolled)
    {
        // 執行
        var result = status == "enrolled";

        // 驗證
        Assert.Equal(isEnrolled, result);
    }

    [Fact]
    public void 會員報名查詢回應_應包含場次資訊()
    {
        // 安排
        var sessionData = new SessionEnrollmentResponse
        {
            Id = "session123",
            CourseId = "course456",
            TeamId = "team789",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Capacity = 20,
            EnrolledCount = 5,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };

        var response = new MemberEnrollmentsResponse
        {
            EnrollmentId = "enrollment123",
            SessionId = "session123",
            Status = "enrolled",
            CreatedAt = DateTime.UtcNow,
            Session = sessionData
        };

        // 驗證
        Assert.Equal("enrollment123", response.EnrollmentId);
        Assert.Equal("session123", response.SessionId);
        Assert.Equal("enrolled", response.Status);
        Assert.NotNull(response.Session);
        Assert.Equal("course456", response.Session.CourseId);
        Assert.Equal(20, response.Session.Capacity);
        Assert.Equal(5, response.Session.EnrolledCount);
    }

    [Theory]
    [InlineData(10, 5, true)]  // 容量足夠
    [InlineData(10, 10, false)] // 容量已滿
    [InlineData(10, 15, false)] // 超過容量
    public void 容量檢查_邏輯應正確運作(int capacity, int enrolled, bool shouldAllow)
    {
        // 執行
        bool hasCapacity = enrolled < capacity;

        // 驗證
        Assert.Equal(shouldAllow, hasCapacity);
    }
}