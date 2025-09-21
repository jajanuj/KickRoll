using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;

namespace KickRoll.Api.Controllers;

[FirestoreData]
public class AttendanceRecord
{
   [FirestoreProperty]
   public string SessionId { get; set; }

   [FirestoreProperty]
   public string MemberId { get; set; }

   [FirestoreProperty]
   public string Status { get; set; } // Present / Absent
}

[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
   private readonly FirestoreDb _db;

   public AttendanceController(FirestoreDb db)
   {
      _db = db;
   }

   [HttpPost("submit")]
   public async Task<IActionResult> SubmitAttendance([FromBody] List<AttendanceRecord> records)
   {
      var batch = _db.StartBatch();

      foreach (var record in records)
      {
         // 用 SessionId + MemberId 組成唯一的文件 ID
         var docId = $"{record.SessionId}_{record.MemberId}";
         var docRef = _db.Collection("attendance").Document(docId);

         batch.Set(docRef, record, SetOptions.Overwrite); // 覆蓋同一筆
      }

      await batch.CommitAsync();
      return Ok(new { message = "Attendance submitted", count = records.Count });
   }
}
