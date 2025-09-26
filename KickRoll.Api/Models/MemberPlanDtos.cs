using Google.Cloud.Firestore;

namespace KickRoll.Api.Models;

// DTO classes for API requests/responses (camelCase for JSON)
public class CreateMemberPlanRequest
{
    public string Type { get; set; } = default!; // "credit_pack" | "time_pass"
    public string Name { get; set; } = default!;
    public int? TotalCredits { get; set; }
    public int RemainingCredits { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string Status { get; set; } = "active";
}

public class UpdateMemberPlanRequest
{
    public string? Type { get; set; }
    public string? Name { get; set; }
    public int? TotalCredits { get; set; }
    public int? RemainingCredits { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? Status { get; set; }
}

public class AdjustCreditsRequest
{
    public int Delta { get; set; } // Can be positive or negative
    public string? Reason { get; set; } // Optional reason for adjustment
}

// Response DTO (will be serialized to camelCase JSON)
public class MemberPlanResponse
{
    public string Id { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int? TotalCredits { get; set; }
    public int RemainingCredits { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}