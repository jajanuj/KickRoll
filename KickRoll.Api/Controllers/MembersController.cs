using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;

namespace KickRoll.Api.Controllers;

[FirestoreData]
public class Member
{
   [FirestoreProperty] public string MemberId { get; set; }
   [FirestoreProperty] public string Name { get; set; }
   [FirestoreProperty] public string Phone { get; set; }
   [FirestoreProperty] public string Gender { get; set; }
   [FirestoreProperty] public string Status { get; set; }
   [FirestoreProperty] public string TeamId { get; set; }
   [FirestoreProperty] public List<string> TeamIds { get; set; } = new();
   [FirestoreProperty] public DateTime? Birthdate { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
   private readonly FirestoreDb _db;

   public MembersController(FirestoreDb db)
   {
      _db = db;
   }

   [HttpGet("byTeam/{teamId}")]
   public async Task<IActionResult> GetMembersByTeam(string teamId)
   {
      var snapshot = await _db.Collection("members")
          .WhereArrayContains("teamIds", teamId)
          .GetSnapshotAsync();

      var members = snapshot.Documents.Select(doc => new
      {
         MemberId = doc.Id,
         Name = doc.ContainsField("name") ? doc.GetValue<string>("name") : "(未命名)"
      }).ToList();

      return Ok(members);
   }

   //[HttpPut("{memberId}")]
   //public async Task<IActionResult> UpdateMember(string memberId, [FromBody] Member updatedMember)
   //{
   //   if (updatedMember.TeamIds == null)
   //   {
   //      updatedMember.TeamIds = new List<string>();
   //   }

   //   if (updatedMember.Birthdate.HasValue)
   //   {
   //      updatedMember.Birthdate = DateTime.SpecifyKind(updatedMember.Birthdate.Value, DateTimeKind.Utc);
   //   }

   //   var docRef = _db.Collection("members").Document(memberId);
   //   await docRef.SetAsync(updatedMember, SetOptions.Overwrite);
   //   return Ok(new { message = "Member updated", memberId });
   //}

   [HttpPut("{memberId}")]
   public async Task<IActionResult> UpdateMember(string memberId, [FromBody] Member member)
   {
      var docRef = _db.Collection("members").Document(memberId);

      var updates = new Dictionary<string, object>();

      if (!string.IsNullOrWhiteSpace(member.Name))
         updates["name"] = member.Name;

      if (!string.IsNullOrWhiteSpace(member.Phone))
         updates["phone"] = member.Phone;

      if (!string.IsNullOrWhiteSpace(member.Gender))
         updates["gender"] = member.Gender;

      if (member.Birthdate.HasValue)
         updates["birthdate"] = member.Birthdate.Value;

      if (!string.IsNullOrWhiteSpace(member.Status))
         updates["status"] = member.Status;

      updates["teamId"] = member.TeamId;
      updates["teamIds"] = member.TeamIds ?? new List<string>();

      await docRef.SetAsync(updates, SetOptions.MergeAll);

      return Ok(new { success = true });
   }

   //[HttpGet("list")]
   //public async Task<IActionResult> GetAllMembers()
   //{
   //   var snapshot = await _db.Collection("members").GetSnapshotAsync();

   //   var members = snapshot.Documents.Select(doc => new
   //   {
   //      MemberId = doc.Id,
   //      Name = doc.ContainsField("name") ? doc.GetValue<string>("name") : "(未命名)",
   //      Status = doc.ContainsField("status") ? doc.GetValue<string>("status") : "unknown"
   //   }).ToList();

   //   return Ok(members);
   //}

   [HttpGet("list")]
   public async Task<IActionResult> GetAllMembers()
   {
      var snapshot = await _db.Collection("members").GetSnapshotAsync();

      foreach (var doc in snapshot.Documents)
      {
         //Console.WriteLine($"[DEBUG] Member Doc ID: {doc.Id}, Exists: {doc.Exists}");
         if (doc.Exists)
         {
            var dict = doc.ToDictionary();
            //Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(dict));
         }
      }

      var members = snapshot.Documents
         .Select(doc => new
         {
            MemberId = doc.Id,
            Name = doc.ContainsField("name") ? doc.GetValue<string>("name") : "(未命名)",
            Status = doc.ContainsField("status") ? doc.GetValue<string>("status") : "unknown"
         })
         .ToList();

      return Ok(members);
   }

   [HttpGet("{memberId}")]
   public async Task<IActionResult> GetMember(string memberId)
   {
      var docRef = _db.Collection("members").Document(memberId);
      var snapshot = await docRef.GetSnapshotAsync();

      if (!snapshot.Exists)
      {
         return NotFound();
      }

      Console.WriteLine($"[DEBUG] Member snapshot ID: {snapshot.Id}, Date: {snapshot.GetValue<DateTime>("birthdate")}");

      var member = new
      {
         MemberId = snapshot.Id,
         Name = snapshot.ContainsField("name") ? snapshot.GetValue<string>("name") : "",
         Phone = snapshot.ContainsField("phone") ? snapshot.GetValue<string>("phone") : "",
         Gender = snapshot.ContainsField("gender") ? snapshot.GetValue<string>("gender") : "",
         Birthdate = snapshot.ContainsField("birthdate") ? snapshot.GetValue<DateTime>("birthdate") : DateTime.MinValue,
         Status = snapshot.ContainsField("status") ? snapshot.GetValue<string>("status") : "unknown",
         TeamId = snapshot.ContainsField("teamId") ? snapshot.GetValue<string>("teamId") : "unknown",
         TeamIds = snapshot.ContainsField("teamIds") ? snapshot.GetValue<List<string>>("teamIds") : new List<string>()
      };

      return Ok(member);
   }
}
