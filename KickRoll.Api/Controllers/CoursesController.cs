using Google.Cloud.Firestore;
using KickRoll.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace KickRoll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly FirestoreDb _db;

    public CoursesController(FirestoreDb db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourse([FromBody] Course course)
    {
        if (string.IsNullOrWhiteSpace(course.Name))
        {
            return BadRequest(new { message = "課程名稱不可為空" });
        }

        // Generate course ID if not provided
        if (string.IsNullOrWhiteSpace(course.CourseId))
        {
            course.CourseId = Guid.NewGuid().ToString("N");
        }

        // Set default values
        if (string.IsNullOrWhiteSpace(course.Status))
        {
            course.Status = "Active";
        }

        // Ensure dates are UTC
        if (course.StartDate.HasValue)
        {
            course.StartDate = DateTime.SpecifyKind(course.StartDate.Value, DateTimeKind.Utc);
        }
        if (course.EndDate.HasValue)
        {
            course.EndDate = DateTime.SpecifyKind(course.EndDate.Value, DateTimeKind.Utc);
        }

        var docRef = _db.Collection("courses").Document(course.CourseId);
        await docRef.SetAsync(course);

        return Ok(course);
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetAllCourses()
    {
        var snapshot = await _db.Collection("courses").GetSnapshotAsync();

        var courses = snapshot.Documents.Select(doc => new
        {
            CourseId = doc.Id,
            Name = doc.ContainsField("name") ? doc.GetValue<string>("name") : "(未命名)",
            Description = doc.ContainsField("description") ? doc.GetValue<string>("description") : "",
            InstructorId = doc.ContainsField("instructorId") ? doc.GetValue<string>("instructorId") : "",
            Capacity = doc.ContainsField("capacity") ? doc.GetValue<int>("capacity") : 0,
            Status = doc.ContainsField("status") ? doc.GetValue<string>("status") : "Active"
        }).ToList();

        return Ok(courses);
    }

    [HttpGet("{courseId}")]
    public async Task<IActionResult> GetCourse(string courseId)
    {
        var docRef = _db.Collection("courses").Document(courseId);
        var snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists)
        {
            return NotFound();
        }

        var course = new
        {
            CourseId = snapshot.Id,
            Name = snapshot.ContainsField("name") ? snapshot.GetValue<string>("name") : "",
            Description = snapshot.ContainsField("description") ? snapshot.GetValue<string>("description") : "",
            InstructorId = snapshot.ContainsField("instructorId") ? snapshot.GetValue<string>("instructorId") : "",
            Capacity = snapshot.ContainsField("capacity") ? snapshot.GetValue<int>("capacity") : 0,
            Status = snapshot.ContainsField("status") ? snapshot.GetValue<string>("status") : "Active",
            StartDate = snapshot.ContainsField("startDate") ? snapshot.GetValue<DateTime?>("startDate") : null,
            EndDate = snapshot.ContainsField("endDate") ? snapshot.GetValue<DateTime?>("endDate") : null
        };

        return Ok(course);
    }

    [HttpPost("{courseId}/members/{memberId}")]
    public async Task<IActionResult> JoinCourse(string courseId, string memberId)
    {
        if (string.IsNullOrWhiteSpace(courseId) || string.IsNullOrWhiteSpace(memberId))
        {
            return BadRequest(new { message = "課程 ID 與成員 ID 不可為空" });
        }

        // Check if member exists
        var memberRef = _db.Collection("members").Document(memberId);
        var memberSnapshot = await memberRef.GetSnapshotAsync();
        
        if (!memberSnapshot.Exists)
        {
            return NotFound(new { message = "成員不存在" });
        }

        // Check if course exists
        var courseRef = _db.Collection("courses").Document(courseId);
        var courseSnapshot = await courseRef.GetSnapshotAsync();
        
        if (!courseSnapshot.Exists)
        {
            return NotFound(new { message = "課程不存在" });
        }

        // Check if member already enrolled in this course
        var memberCourseRef = _db.Collection("members").Document(memberId)
            .Collection("courses").Document(courseId);
        var existingEnrollment = await memberCourseRef.GetSnapshotAsync();

        if (existingEnrollment.Exists)
        {
            return BadRequest(new { message = "成員已經加入此課程" });
        }

        // Create enrollment record
        var enrollmentData = new
        {
            CourseId = courseId,
            MemberId = memberId,
            JoinedAt = DateTime.UtcNow,
            Status = "Active"
        };

        await memberCourseRef.SetAsync(enrollmentData);

        return Ok(new { message = "成功加入課程", courseId, memberId });
    }

    [HttpGet("members/{memberId}")]
    public async Task<IActionResult> GetMemberCourses(string memberId)
    {
        if (string.IsNullOrWhiteSpace(memberId))
        {
            return BadRequest(new { message = "成員 ID 不可為空" });
        }

        var memberCoursesSnapshot = await _db.Collection("members").Document(memberId)
            .Collection("courses").GetSnapshotAsync();

        var courses = new List<object>();

        foreach (var doc in memberCoursesSnapshot.Documents)
        {
            var courseId = doc.GetValue<string>("CourseId");
            var courseRef = _db.Collection("courses").Document(courseId);
            var courseSnapshot = await courseRef.GetSnapshotAsync();

            if (courseSnapshot.Exists)
            {
                courses.Add(new
                {
                    CourseId = courseId,
                    Name = courseSnapshot.ContainsField("name") ? courseSnapshot.GetValue<string>("name") : "(未命名)",
                    Description = courseSnapshot.ContainsField("description") ? courseSnapshot.GetValue<string>("description") : "",
                    JoinedAt = doc.ContainsField("JoinedAt") ? doc.GetValue<DateTime>("JoinedAt") : DateTime.MinValue,
                    Status = doc.ContainsField("Status") ? doc.GetValue<string>("Status") : "Active"
                });
            }
        }

        return Ok(courses);
    }

    [HttpDelete("{courseId}/members/{memberId}")]
    public async Task<IActionResult> LeaveCourse(string courseId, string memberId)
    {
        if (string.IsNullOrWhiteSpace(courseId) || string.IsNullOrWhiteSpace(memberId))
        {
            return BadRequest(new { message = "課程 ID 與成員 ID 不可為空" });
        }

        var memberCourseRef = _db.Collection("members").Document(memberId)
            .Collection("courses").Document(courseId);

        var snapshot = await memberCourseRef.GetSnapshotAsync();
        if (!snapshot.Exists)
        {
            return NotFound(new { message = "成員未加入此課程" });
        }

        await memberCourseRef.DeleteAsync();

        return Ok(new { message = "成功退出課程", courseId, memberId });
    }
}