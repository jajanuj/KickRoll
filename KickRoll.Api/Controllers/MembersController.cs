using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;

namespace KickRoll.Api.Controllers;

[FirestoreData]
public class Member
{
   [FirestoreProperty(Name = "MemberId")] public string MemberId { get; set; }
   [FirestoreProperty(Name = "Name")] public string Name { get; set; }
   [FirestoreProperty(Name = "Phone")] public string Phone { get; set; }
   [FirestoreProperty(Name = "Gender")] public string Gender { get; set; }
   [FirestoreProperty(Name = "Status")] public string Status { get; set; }
   [FirestoreProperty(Name = "TeamId")] public string TeamId { get; set; }
   [FirestoreProperty(Name = "TeamIds")] public List<string> TeamIds { get; set; } = new();
   [FirestoreProperty(Name = "Birthdate")] public DateTime? Birthdate { get; set; }
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
          .WhereArrayContains("TeamIds", teamId)
          .GetSnapshotAsync();

      var members = snapshot.Documents.Select(doc => new
      {
         MemberId = doc.Id,
         Name = doc.ContainsField("Name") ? doc.GetValue<string>("Name") : "(未命名)"
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
      // Check for duplicate name (excluding the current member being updated)
      if (!string.IsNullOrWhiteSpace(member.Name))
      {
         var existingMembersSnapshot = await _db.Collection("members").GetSnapshotAsync();
         var duplicateByName = existingMembersSnapshot.Documents.FirstOrDefault(doc =>
            doc.Id != memberId && // Exclude current member
            doc.ContainsField("Name") && 
            string.Equals(doc.GetValue<string>("Name"), member.Name, StringComparison.OrdinalIgnoreCase));

         if (duplicateByName != null)
         {
            return BadRequest(new { error = $"成員姓名 '{member.Name}' 已存在" });
         }
      }

      var docRef = _db.Collection("members").Document(memberId);

      var updates = new Dictionary<string, object>();

      if (!string.IsNullOrWhiteSpace(member.Name))
         updates["Name"] = member.Name;

      if (!string.IsNullOrWhiteSpace(member.Phone))
         updates["Phone"] = member.Phone;

      if (!string.IsNullOrWhiteSpace(member.Gender))
         updates["Gender"] = member.Gender;

      if (member.Birthdate.HasValue)
         updates["Birthdate"] = member.Birthdate.Value;

      if (!string.IsNullOrWhiteSpace(member.Status))
         updates["Status"] = member.Status;

      updates["TeamId"] = member.TeamId;
      updates["TeamIds"] = member.TeamIds ?? new List<string>();

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
            Name = doc.ContainsField("Name") ? doc.GetValue<string>("Name") : "(未命名)",
            Status = doc.ContainsField("Status") ? doc.GetValue<string>("Status") : "unknown"
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
         // Check for duplicate members by name only
         // Phone numbers can be shared (e.g., family members using parent's phone)
         var existingMembersSnapshot = await _db.Collection("members").GetSnapshotAsync();
         var duplicateByName = existingMembersSnapshot.Documents.FirstOrDefault(doc =>
            doc.ContainsField("Name") && 
            string.Equals(doc.GetValue<string>("Name"), request.Name, StringComparison.OrdinalIgnoreCase));

         if (duplicateByName != null)
         {
            return BadRequest(new { error = $"成員姓名 '{request.Name}' 已存在" });
         }

         // Generate new document ID
         var newDocRef = _db.Collection("members").Document();
         
         var newMember = new Dictionary<string, object>
         {
            ["Name"] = request.Name,
            ["Phone"] = request.Phone,
            ["Gender"] = request.Gender ?? "",
            ["Status"] = request.Status ?? "active",
            ["TeamId"] = request.TeamId ?? "",
            ["TeamIds"] = request.TeamIds ?? new List<string>(),
            ["Birthdate"] = request.Birthdate ?? DateTime.UtcNow
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

      Console.WriteLine($"[DEBUG] Member snapshot ID: {snapshot.Id}, Date: {snapshot.GetValue<DateTime>("Birthdate")}");

      var member = new
      {
         MemberId = snapshot.Id,
         Name = snapshot.ContainsField("Name") ? snapshot.GetValue<string>("Name") : "",
         Phone = snapshot.ContainsField("Phone") ? snapshot.GetValue<string>("Phone") : "",
         Gender = snapshot.ContainsField("Gender") ? snapshot.GetValue<string>("Gender") : "",
         Birthdate = snapshot.ContainsField("Birthdate") ? snapshot.GetValue<DateTime>("Birthdate") : DateTime.MinValue,
         Status = snapshot.ContainsField("Status") ? snapshot.GetValue<string>("Status") : "unknown",
         TeamId = snapshot.ContainsField("TeamId") ? snapshot.GetValue<string>("TeamId") : "unknown",
         TeamIds = snapshot.ContainsField("TeamIds") ? snapshot.GetValue<List<string>>("TeamIds") : new List<string>()
      };

      return Ok(member);
   }
}
