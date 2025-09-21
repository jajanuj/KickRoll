using Google.Cloud.Firestore;

namespace KickRoll.Api.Models;

[FirestoreData]
public class Team
{
   #region Properties

   [FirestoreDocumentId] public string? TeamId { get; set; }

   [FirestoreProperty] public string? Name { get; set; }

   [FirestoreProperty] public string? Location { get; set; }

   [FirestoreProperty] public int Capacity { get; set; }

   [FirestoreProperty] public List<string> CoachIds { get; set; } = new();

   [FirestoreProperty] public string? ScheduleHints { get; set; }

   #endregion
}