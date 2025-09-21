using Google.Cloud.Firestore;

namespace KickRoll.Api.Models;

[FirestoreData]
public class Course
{
    [FirestoreDocumentId]
    public string? CourseId { get; set; }

    [FirestoreProperty]
    public string? Name { get; set; }

    [FirestoreProperty]
    public string? Description { get; set; }

    [FirestoreProperty]
    public string? InstructorId { get; set; }

    [FirestoreProperty]
    public int Capacity { get; set; }

    [FirestoreProperty]
    public string Status { get; set; } = "Active";

    [FirestoreProperty]
    public DateTime? StartDate { get; set; }

    [FirestoreProperty]
    public DateTime? EndDate { get; set; }
}