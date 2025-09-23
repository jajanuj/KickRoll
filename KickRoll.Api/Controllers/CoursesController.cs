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

    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok(new { message = "API is working", timestamp = DateTime.UtcNow });
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourse([FromBody] Course course)
    {
        try
        {
            Console.WriteLine($"[DEBUG] CreateCourse called");
            Console.WriteLine($"[DEBUG] Request Content-Type: {Request.ContentType}");
            Console.WriteLine($"[DEBUG] Course is null: {course == null}");
            
            if (course != null)
            {
                Console.WriteLine($"[DEBUG] Course object: Name='{course.Name}', CourseId='{course.CourseId}', Status='{course.Status}'");
                Console.WriteLine($"[DEBUG] Course serialized: {System.Text.Json.JsonSerializer.Serialize(course)}");
            }
            else
            {
                // Try to read raw request body for debugging
                Request.EnableBuffering();
                Request.Body.Position = 0;
                var reader = new StreamReader(Request.Body);
                var rawJson = await reader.ReadToEndAsync();
                Console.WriteLine($"[DEBUG] Raw request body: {rawJson}");
                Request.Body.Position = 0;
            }
            
            if (course == null)
            {
                return BadRequest(new { message = "課程資料不可為空 - 請檢查 JSON 格式" });
            }

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

            Console.WriteLine($"[DEBUG] Creating course in Firestore: {course.CourseId}");
            var docRef = _db.Collection("courses").Document(course.CourseId);
            await docRef.SetAsync(course);

            Console.WriteLine($"[DEBUG] Course created successfully: {course.CourseId}");
            return Ok(course);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] CreateCourse failed: {ex.Message}");
            Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { message = $"建立課程失敗: {ex.Message}" });
        }
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetAllCourses()
    {
        try
        {
            var snapshot = await _db.Collection("courses").GetSnapshotAsync();
            
            Console.WriteLine($"[DEBUG] Found {snapshot.Documents.Count} course documents");

            var courses = new List<object>();
            
            foreach (var doc in snapshot.Documents)
            {
                Console.WriteLine($"[DEBUG] Course doc ID: {doc.Id}, Exists: {doc.Exists}");
                
                if (doc.Exists)
                {
                    // Print all fields for debugging
                    var fields = doc.ToDictionary();
                    Console.WriteLine($"[DEBUG] Course fields: {string.Join(", ", fields.Keys)}");
                    
                    courses.Add(new
                    {
                        CourseId = doc.Id,
                        Name = doc.ContainsField("name") ? doc.GetValue<string>("name") : 
                               doc.ContainsField("Name") ? doc.GetValue<string>("Name") : "(未命名)",
                        Description = doc.ContainsField("description") ? doc.GetValue<string>("description") :
                                    doc.ContainsField("Description") ? doc.GetValue<string>("Description") : "",
                        InstructorId = doc.ContainsField("instructorId") ? doc.GetValue<string>("instructorId") :
                                     doc.ContainsField("InstructorId") ? doc.GetValue<string>("InstructorId") : "",
                        Capacity = doc.ContainsField("capacity") ? doc.GetValue<int>("capacity") :
                                 doc.ContainsField("Capacity") ? doc.GetValue<int>("Capacity") : 0,
                        Status = doc.ContainsField("status") ? doc.GetValue<string>("status") :
                               doc.ContainsField("Status") ? doc.GetValue<string>("Status") : "Active",
                        StartDate = doc.ContainsField("startDate") ? doc.GetValue<DateTime?>("startDate") :
                                  doc.ContainsField("StartDate") ? doc.GetValue<DateTime?>("StartDate") : null,
                        EndDate = doc.ContainsField("endDate") ? doc.GetValue<DateTime?>("endDate") :
                                doc.ContainsField("EndDate") ? doc.GetValue<DateTime?>("EndDate") : null
                    });
                }
            }

            Console.WriteLine($"[DEBUG] Returning {courses.Count} courses");
            return Ok(courses);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] GetAllCourses failed: {ex.Message}");
            return StatusCode(500, new { message = $"載入課程失敗: {ex.Message}" });
        }
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

        try
        {
            Console.WriteLine($"[DEBUG] Getting courses for member: {memberId}");
            
            var memberCoursesSnapshot = await _db.Collection("members").Document(memberId)
                .Collection("courses").GetSnapshotAsync();

            Console.WriteLine($"[DEBUG] Found {memberCoursesSnapshot.Documents.Count} enrolled courses for member {memberId}");

            var courses = new List<object>();

            foreach (var doc in memberCoursesSnapshot.Documents)
            {
                var courseId = doc.GetValue<string>("CourseId");
                Console.WriteLine($"[DEBUG] Processing enrolled course: {courseId}");
                
                var courseRef = _db.Collection("courses").Document(courseId);
                var courseSnapshot = await courseRef.GetSnapshotAsync();

                if (courseSnapshot.Exists)
                {
                    courses.Add(new
                    {
                        CourseId = courseId,
                        Name = courseSnapshot.ContainsField("name") ? courseSnapshot.GetValue<string>("name") :
                               courseSnapshot.ContainsField("Name") ? courseSnapshot.GetValue<string>("Name") : "(未命名)",
                        Description = courseSnapshot.ContainsField("description") ? courseSnapshot.GetValue<string>("description") :
                                    courseSnapshot.ContainsField("Description") ? courseSnapshot.GetValue<string>("Description") : "",
                        JoinedAt = doc.ContainsField("JoinedAt") ? doc.GetValue<DateTime>("JoinedAt") : DateTime.MinValue,
                        Status = doc.ContainsField("Status") ? doc.GetValue<string>("Status") : "Active"
                    });
                }
                else
                {
                    Console.WriteLine($"[WARNING] Course {courseId} not found in courses collection");
                }
            }

            Console.WriteLine($"[DEBUG] Returning {courses.Count} enrolled courses");
            return Ok(courses);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] GetMemberCourses failed: {ex.Message}");
            return StatusCode(500, new { message = $"載入成員課程失敗: {ex.Message}" });
        }
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