using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace KickRoll.Api.Models;

[FirestoreData]
public class Course
{
    [FirestoreDocumentId]
    [JsonPropertyName("courseId")]
    public string? CourseId { get; set; }

    [FirestoreProperty("name")]
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [FirestoreProperty("description")]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [FirestoreProperty("instructorId")]
    [JsonPropertyName("instructorId")]
    public string? InstructorId { get; set; }

    [FirestoreProperty("capacity")]
    [JsonPropertyName("capacity")]
    public int Capacity { get; set; }

    [FirestoreProperty("status")]
    [JsonPropertyName("status")]
    public string Status { get; set; } = "Active";

    [FirestoreProperty("startDate")]
    [JsonPropertyName("startDate")]
    public DateTime? StartDate { get; set; }

    [FirestoreProperty("endDate")]
    [JsonPropertyName("endDate")]
    public DateTime? EndDate { get; set; }
}