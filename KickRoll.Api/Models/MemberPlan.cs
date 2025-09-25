using Google.Cloud.Firestore;

namespace KickRoll.Api.Models;

[FirestoreData]
public class MemberPlan
{
    [FirestoreDocumentId] 
    public string Id { get; set; } = default!;

    [FirestoreProperty("Type")] 
    public string Type { get; set; } = default!;

    [FirestoreProperty("Name")] 
    public string Name { get; set; } = default!;

    [FirestoreProperty("TotalCredits")] 
    public int? TotalCredits { get; set; }

    [FirestoreProperty("RemainingCredits")] 
    public int RemainingCredits { get; set; }

    [FirestoreProperty("ValidFrom")] 
    public Timestamp? ValidFrom { get; set; }

    [FirestoreProperty("ValidUntil")] 
    public Timestamp? ValidUntil { get; set; }

    [FirestoreProperty("Status")] 
    public string Status { get; set; } = "active";

    [FirestoreProperty("CreatedAt")] 
    public Timestamp CreatedAt { get; set; }

    [FirestoreProperty("UpdatedAt")] 
    public Timestamp UpdatedAt { get; set; }
}