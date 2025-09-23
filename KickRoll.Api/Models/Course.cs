using Google.Cloud.Firestore;

namespace KickRoll.Api.Models;

[FirestoreData]
public class Course
{
    [FirestoreDocumentId]
    public string? CourseId { get; set; }

    [FirestoreProperty("name")]
    public string? Name { get; set; }

    [FirestoreProperty("description")]
    public string? Description { get; set; }

    [FirestoreProperty("instructorId")]
    public string? InstructorId { get; set; }

    [FirestoreProperty("capacity")]
    public int Capacity { get; set; }

    [FirestoreProperty("status")]
    public string Status { get; set; } = "Active";

    [FirestoreProperty("startDate")]
    public DateTime? StartDate { get; set; }

    [FirestoreProperty("endDate")]
    public DateTime? EndDate { get; set; }
}