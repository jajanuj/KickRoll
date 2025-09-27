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
            EnrolledCount = doc.ContainsField("EnrolledCount") ? doc.GetValue<int>("EnrolledCount") : 0,
            Status = doc.ContainsField("Status") ? doc.GetValue<string>("Status") : "Unknown"
         });
      }

      return Ok(sessions);
   }

   // Enrollment endpoints
   
   [HttpPost("{sessionId}/enroll")]
   public async Task<IActionResult> EnrollInSession(string sessionId, [FromBody] EnrollmentRequest request)
   {
      try
      {
         // Validate inputs
         if (string.IsNullOrWhiteSpace(request.MemberId))
         {
            return BadRequest(new { error = "MemberId is required" });
         }

         // Run transaction to ensure atomicity
         var result = await _db.RunTransactionAsync(async transaction =>
         {
            var sessionRef = _db.Collection("class_sessions").Document(sessionId);
            var sessionSnap = await transaction.GetSnapshotAsync(sessionRef);

            if (!sessionSnap.Exists)
            {
               throw new ArgumentException("Session not found");
            }

            // Get session data
            var capacity = sessionSnap.ContainsField("Capacity") ? sessionSnap.GetValue<int>("Capacity") : 0;
            var enrolled = sessionSnap.ContainsField("EnrolledCount") ? sessionSnap.GetValue<int>("EnrolledCount") : 0;

            // Check capacity
            if (enrolled >= capacity)
            {
               throw new CapacityFullException();
            }

            // Check if already enrolled (idempotent)
            var enrollRef = sessionRef.Collection("enrollments").Document(request.MemberId);
            var enrollSnap = await transaction.GetSnapshotAsync(enrollRef);
            
            if (enrollSnap.Exists && enrollSnap.GetValue<string>("Status") == "enrolled")
            {
               throw new AlreadyEnrolledException();
            }

            // Create or update enrollment
            var enrollmentData = new Dictionary<string, object>
            {
               { "MemberId", request.MemberId },
               { "SessionId", sessionId },
               { "Status", "enrolled" },
               { "CreatedAt", Timestamp.GetCurrentTimestamp() }
            };

            transaction.Set(enrollRef, enrollmentData, SetOptions.MergeAll);

            // Increment enrolled count only if it's a new enrollment
            if (!enrollSnap.Exists || enrollSnap.GetValue<string>("Status") != "enrolled")
            {
               transaction.Set(sessionRef, new Dictionary<string, object>
               {
                  { "EnrolledCount", FieldValue.Increment(1) }
               }, SetOptions.MergeAll);
            }

            return new EnrollmentResponse
            {
               Id = request.MemberId,
               MemberId = request.MemberId,
               SessionId = sessionId,
               Status = "enrolled",
               CreatedAt = DateTime.UtcNow
            };
         });

         return Ok(new { success = true, enrollment = result });
      }
      catch (CapacityFullException)
      {
         return BadRequest(new { error = "capacity_full", message = "Session is at full capacity" });
      }
      catch (AlreadyEnrolledException)
      {
         return BadRequest(new { error = "already_enrolled", message = "Member is already enrolled in this session" });
      }
      catch (ArgumentException ex)
      {
         return NotFound(new { error = "session_not_found", message = ex.Message });
      }
      catch (Exception ex)
      {
         return StatusCode(500, new { error = "internal_error", message = $"Failed to enroll: {ex.Message}" });
      }
   }

   [HttpPost("{sessionId}/cancel")]
   public async Task<IActionResult> CancelEnrollment(string sessionId, [FromBody] EnrollmentRequest request)
   {
      try
      {
         // Validate inputs
         if (string.IsNullOrWhiteSpace(request.MemberId))
         {
            return BadRequest(new { error = "MemberId is required" });
         }

         // Run transaction to ensure atomicity
         var result = await _db.RunTransactionAsync(async transaction =>
         {
            var sessionRef = _db.Collection("class_sessions").Document(sessionId);
            var sessionSnap = await transaction.GetSnapshotAsync(sessionRef);

            if (!sessionSnap.Exists)
            {
               throw new ArgumentException("Session not found");
            }

            var enrollRef = sessionRef.Collection("enrollments").Document(request.MemberId);
            var enrollSnap = await transaction.GetSnapshotAsync(enrollRef);

            if (!enrollSnap.Exists || enrollSnap.GetValue<string>("Status") != "enrolled")
            {
               throw new ArgumentException("No active enrollment found");
            }

            // Update enrollment status to cancelled
            transaction.Update(enrollRef, new Dictionary<string, object>
            {
               { "Status", "cancelled" }
            });

            // Decrement enrolled count
            transaction.Set(sessionRef, new Dictionary<string, object>
            {
               { "EnrolledCount", FieldValue.Increment(-1) }
            }, SetOptions.MergeAll);

            return new EnrollmentResponse
            {
               Id = request.MemberId,
               MemberId = request.MemberId,
               SessionId = sessionId,
               Status = "cancelled",
               CreatedAt = enrollSnap.GetValue<Timestamp>("CreatedAt").ToDateTime()
            };
         });

         return Ok(new { success = true, enrollment = result });
      }
      catch (ArgumentException ex)
      {
         return NotFound(new { error = "enrollment_not_found", message = ex.Message });
      }
      catch (Exception ex)
      {
         return StatusCode(500, new { error = "internal_error", message = $"Failed to cancel enrollment: {ex.Message}" });
      }
   }
}
