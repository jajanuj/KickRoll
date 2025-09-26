using Google.Cloud.Firestore;

namespace KickRoll.Api.Models;

[FirestoreData]
public class Enrollment
{
    [FirestoreDocumentId] 
    public string Id { get; set; } = default!;
    
    [FirestoreProperty("MemberId")] 
    public string MemberId { get; set; } = default!;
    
    [FirestoreProperty("SessionId")] 
    public string SessionId { get; set; } = default!;
    
    [FirestoreProperty("Status")] 
    public string Status { get; set; } = "enrolled";
    
    [FirestoreProperty("CreatedAt")] 
    public Timestamp CreatedAt { get; set; }
}