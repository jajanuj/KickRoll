using Google.Cloud.Firestore;
using KickRoll.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace KickRoll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
   private readonly FirestoreDb _db;

   public SessionsController(FirestoreDb db)
   {
      _db = db;
   }

   [HttpPost]
   public async Task<IActionResult> CreateSession([FromBody] ClassSession session)
   {
      session.SessionId = Guid.NewGuid().ToString("N");
      session.StartAt = DateTime.SpecifyKind(session.StartAt, DateTimeKind.Utc);
      session.EndAt = DateTime.SpecifyKind(session.EndAt, DateTimeKind.Utc);

      var docRef = _db.Collection("class_sessions").Document(session.SessionId);
      await docRef.SetAsync(session);

      return Ok(session);
   }

   [HttpGet("list")]
   public async Task<IActionResult> GetSessions()
   {
      var snapshot = await _db.Collection("class_sessions").GetSnapshotAsync();
      var sessions = new List<object>();

      foreach (var doc in snapshot.Documents)
      {
         string teamId = doc.ContainsField("TeamId") ? doc.GetValue<string>("TeamId") : "";
         string teamName = teamId;

         if (!string.IsNullOrEmpty(teamId))
         {
            var teamDoc = await _db.Collection("teams").Document(teamId).GetSnapshotAsync();
            if (teamDoc.Exists && teamDoc.ContainsField("Name"))
            {
               teamName = teamDoc.GetValue<string>("Name");
            }
         }

         sessions.Add(new
         {
            SessionId = doc.Id,
            TeamId = teamId,
            TeamName = teamName,
            StartAt = doc.ContainsField("StartAt") ? doc.GetValue<Timestamp>("StartAt").ToDateTime() : DateTime.MinValue,
            EndAt = doc.ContainsField("EndAt") ? doc.GetValue<Timestamp>("EndAt").ToDateTime() : DateTime.MinValue,
            Location = doc.ContainsField("Location") ? doc.GetValue<string>("Location") : "",
            Capacity = doc.ContainsField("Capacity") ? doc.GetValue<int>("Capacity") : 0,
            Status = doc.ContainsField("Status") ? doc.GetValue<string>("Status") : "Unknown"
         });
      }

      return Ok(sessions);
   }
}
