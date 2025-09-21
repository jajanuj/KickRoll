using Google.Cloud.Firestore;
using KickRoll.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace KickRoll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
   private readonly FirestoreDb _db;

   public TeamsController(FirestoreDb db)
   {
      _db = db;
   }

   [HttpGet("list")]
   public async Task<IActionResult> GetTeams()
   {
      var snapshot = await _db.Collection("teams").GetSnapshotAsync();
      var teams = snapshot.Documents.Select(doc => new
      {
         TeamId = doc.Id,
         Name = doc.ContainsField("Name") ? doc.GetValue<string>("Name") : "(未命名)"
      }).ToList();

      return Ok(teams);
   }
}
