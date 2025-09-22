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

public class CreateMemberRequest
{
   public string Name { get; set; }
   public string Phone { get; set; }
   public string? Gender { get; set; }
   public string? Status { get; set; }
   public string? TeamId { get; set; }
   public List<string>? TeamIds { get; set; }
   public DateTime? Birthdate { get; set; }
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

   [HttpPost]
   public async Task<IActionResult> CreateMember([FromBody] CreateMemberRequest request)
   {
      if (string.IsNullOrWhiteSpace(request.Name))
      {
         return BadRequest(new { error = "姓名不可空白" });
      }

      // Phone validation - must be 10 digits and start with "09"
      if (string.IsNullOrWhiteSpace(request.Phone))
      {
         return BadRequest(new { error = "電話不可空白" });
      }
      
      if (request.Phone.Length != 10 || !request.Phone.StartsWith("09") || !request.Phone.All(char.IsDigit))
      {
         return BadRequest(new { error = "電話號碼格式不正確，必須為10碼且前2碼為09" });
      }

      try
      {
         // Check for duplicate members by name or phone
         var existingMembersSnapshot = await _db.Collection("members").GetSnapshotAsync();
         var duplicateByName = existingMembersSnapshot.Documents.FirstOrDefault(doc =>
            doc.ContainsField("name") && 
            string.Equals(doc.GetValue<string>("name"), request.Name, StringComparison.OrdinalIgnoreCase));
            
         var duplicateByPhone = existingMembersSnapshot.Documents.FirstOrDefault(doc =>
            doc.ContainsField("phone") && 
            doc.GetValue<string>("phone") == request.Phone);

         if (duplicateByName != null)
         {
            return BadRequest(new { error = $"成員姓名 '{request.Name}' 已存在" });
         }

         if (duplicateByPhone != null)
         {
            return BadRequest(new { error = $"電話號碼 '{request.Phone}' 已存在" });
         }

         // Generate new document ID
         var newDocRef = _db.Collection("members").Document();
         
         var newMember = new Dictionary<string, object>
         {
            ["name"] = request.Name,
            ["phone"] = request.Phone,
            ["gender"] = request.Gender ?? "",
            ["status"] = request.Status ?? "active",
            ["teamId"] = request.TeamId ?? "",
            ["teamIds"] = request.TeamIds ?? new List<string>(),
            ["birthdate"] = request.Birthdate ?? DateTime.UtcNow
         };

         await newDocRef.SetAsync(newMember);

         return Ok(new { success = true, memberId = newDocRef.Id, message = "成員新增成功" });
      }
      catch (Exception ex)
      {
         return StatusCode(500, new { error = $"新增成員失敗：{ex.Message}" });
      }
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
