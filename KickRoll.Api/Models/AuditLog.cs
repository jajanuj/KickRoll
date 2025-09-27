using Google.Cloud.Firestore;

namespace KickRoll.Api.Models;

[FirestoreData]
public class AuditLog
{
    [FirestoreDocumentId]
    public string LogId { get; set; } = string.Empty;

    [FirestoreProperty(Name = "ActorUserId")]
    public string ActorUserId { get; set; } = string.Empty;

    [FirestoreProperty(Name = "Action")]
    public string Action { get; set; } = string.Empty;

    [FirestoreProperty(Name = "TargetType")]
    public string TargetType { get; set; } = string.Empty;

    [FirestoreProperty(Name = "TargetId")]
    public string TargetId { get; set; } = string.Empty;

    [FirestoreProperty(Name = "Payload")]
    public Dictionary<string, object>? Payload { get; set; }

    [FirestoreProperty(Name = "At")]
    public Timestamp At { get; set; }

    [FirestoreProperty(Name = "Ip")]
    public string? Ip { get; set; }
}

public class AuditLogResponse
{
    public string LogId { get; set; } = string.Empty;
    public string ActorUserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public Dictionary<string, object>? Payload { get; set; }
    public DateTime At { get; set; }
    public string? Ip { get; set; }
}