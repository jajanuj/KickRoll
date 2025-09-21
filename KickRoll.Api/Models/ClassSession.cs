using Google.Cloud.Firestore;

namespace KickRoll.Api.Models;

[FirestoreData]
public class ClassSession
{
    [FirestoreDocumentId]
    public string? SessionId { get; set; }

    [FirestoreProperty]
    public string? TeamId { get; set; }

    [FirestoreProperty]
    public List<string> CoachIds { get; set; } = new();

    [FirestoreProperty]
    public DateTime StartAt { get; set; }

    [FirestoreProperty]
    public DateTime EndAt { get; set; }

    [FirestoreProperty]
    public string? Location { get; set; }

    [FirestoreProperty]
    public int Capacity { get; set; }

    [FirestoreProperty]
    public string Status { get; set; } = "Scheduled";
}
