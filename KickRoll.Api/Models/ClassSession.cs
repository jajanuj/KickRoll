using Google.Cloud.Firestore;

namespace KickRoll.Api.Models;

[FirestoreData]
public class ClassSession
{
    [FirestoreDocumentId]
    public string? SessionId { get; set; }

    [FirestoreProperty(Name = "TeamId")]
    public string? TeamId { get; set; }

    [FirestoreProperty(Name = "CoachIds")]
    public List<string> CoachIds { get; set; } = new();

    [FirestoreProperty(Name = "StartAt")]
    public DateTime StartAt { get; set; }

    [FirestoreProperty(Name = "EndAt")]
    public DateTime EndAt { get; set; }

    [FirestoreProperty(Name = "Location")]
    public string? Location { get; set; }

    [FirestoreProperty(Name = "Capacity")]
    public int Capacity { get; set; }

    [FirestoreProperty(Name = "Status")]
    public string Status { get; set; } = "Scheduled";
}
