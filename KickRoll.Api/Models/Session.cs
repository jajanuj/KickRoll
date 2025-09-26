using Google.Cloud.Firestore;

namespace KickRoll.Api.Models;

[FirestoreData]
public class Session
{
    [FirestoreDocumentId] 
    public string Id { get; set; } = default!;
    
    [FirestoreProperty("CourseId")] 
    public string CourseId { get; set; } = default!;
    
    [FirestoreProperty("TeamId")] 
    public string TeamId { get; set; } = default!;
    
    [FirestoreProperty("StartTime")] 
    public Timestamp StartTime { get; set; }
    
    [FirestoreProperty("EndTime")] 
    public Timestamp EndTime { get; set; }
    
    [FirestoreProperty("Capacity")] 
    public int Capacity { get; set; }
    
    [FirestoreProperty("EnrolledCount")] 
    public int EnrolledCount { get; set; }
    
    [FirestoreProperty("CreatedAt")] 
    public Timestamp CreatedAt { get; set; }
    
    [FirestoreProperty("UpdatedAt")] 
    public Timestamp UpdatedAt { get; set; }
}