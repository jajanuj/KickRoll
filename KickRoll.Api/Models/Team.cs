using Google.Cloud.Firestore;

namespace KickRoll.Api.Models;

[FirestoreData]
public class Team
{
   #region Properties

   [FirestoreDocumentId] public string? TeamId { get; set; }

   [FirestoreProperty(Name = "Name")] public string? Name { get; set; }

   [FirestoreProperty(Name = "Location")] public string? Location { get; set; }

   [FirestoreProperty(Name = "Capacity")] public int Capacity { get; set; }

   [FirestoreProperty(Name = "CoachIds")] public List<string> CoachIds { get; set; } = new();

   [FirestoreProperty(Name = "ScheduleHints")] public string? ScheduleHints { get; set; }

   #endregion
}