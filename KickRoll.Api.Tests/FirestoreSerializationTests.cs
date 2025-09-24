using Google.Cloud.Firestore;
using KickRoll.Api.Controllers;
using KickRoll.Api.Models;
using Newtonsoft.Json;
using System.Reflection;
using System.Text.Json;
using Xunit;

namespace KickRoll.Api.Tests;

public class FirestoreSerializationTests
{
    [Fact]
    public void Member_Should_Serialize_With_PascalCase_Field_Names()
    {
        // Arrange
        var member = new Member
        {
            MemberId = "M001",
            Name = "John Doe",
            Phone = "0912345678",
            Gender = "Male",
            Status = "Active",
            TeamId = "T001",
            TeamIds = new List<string> { "T001", "T002" },
            Birthdate = DateTime.Now
        };

        // Act - Create a Firestore document representation
        var docData = new Dictionary<string, object>();
        var memberType = typeof(Member);
        
        foreach (var property in memberType.GetProperties())
        {
            var firestoreProperty = property.GetCustomAttribute<FirestorePropertyAttribute>();
            if (firestoreProperty != null)
            {
                var fieldName = firestoreProperty.Name ?? property.Name;
                var value = property.GetValue(member);
                if (value != null)
                {
                    docData[fieldName] = value;
                }
            }
        }

        // Assert - All field names should be PascalCase
        Assert.Contains("Name", docData.Keys);
        Assert.Contains("Phone", docData.Keys);
        Assert.Contains("Gender", docData.Keys);
        Assert.Contains("Status", docData.Keys);
        Assert.Contains("TeamId", docData.Keys);
        Assert.Contains("TeamIds", docData.Keys);
        Assert.Contains("Birthdate", docData.Keys);

        // Verify no camelCase field names exist
        Assert.DoesNotContain("name", docData.Keys);
        Assert.DoesNotContain("phone", docData.Keys);
        Assert.DoesNotContain("gender", docData.Keys);
        Assert.DoesNotContain("status", docData.Keys);
        Assert.DoesNotContain("teamId", docData.Keys);
        Assert.DoesNotContain("teamIds", docData.Keys);
        Assert.DoesNotContain("birthdate", docData.Keys);
    }

    [Fact]
    public void Team_Should_Serialize_With_PascalCase_Field_Names()
    {
        // Arrange
        var team = new Team
        {
            Name = "Team Alpha",
            Location = "Stadium A",
            Capacity = 20,
            CoachIds = new List<string> { "C001", "C002" },
            ScheduleHints = "Weekends only"
        };

        // Act
        var docData = new Dictionary<string, object>();
        var teamType = typeof(Team);
        
        foreach (var property in teamType.GetProperties())
        {
            var firestoreProperty = property.GetCustomAttribute<FirestorePropertyAttribute>();
            if (firestoreProperty != null)
            {
                var fieldName = firestoreProperty.Name ?? property.Name;
                var value = property.GetValue(team);
                if (value != null)
                {
                    docData[fieldName] = value;
                }
            }
        }

        // Assert
        Assert.Contains("Name", docData.Keys);
        Assert.Contains("Location", docData.Keys);
        Assert.Contains("Capacity", docData.Keys);
        Assert.Contains("CoachIds", docData.Keys);
        Assert.Contains("ScheduleHints", docData.Keys);
    }

    [Fact]
    public void ClassSession_Should_Serialize_With_PascalCase_Field_Names()
    {
        // Arrange
        var session = new ClassSession
        {
            TeamId = "T001",
            CoachIds = new List<string> { "C001" },
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(2),
            Location = "Field 1",
            Capacity = 20,
            Status = "Scheduled"
        };

        // Act
        var docData = new Dictionary<string, object>();
        var sessionType = typeof(ClassSession);
        
        foreach (var property in sessionType.GetProperties())
        {
            var firestoreProperty = property.GetCustomAttribute<FirestorePropertyAttribute>();
            if (firestoreProperty != null)
            {
                var fieldName = firestoreProperty.Name ?? property.Name;
                var value = property.GetValue(session);
                if (value != null)
                {
                    docData[fieldName] = value;
                }
            }
        }

        // Assert
        Assert.Contains("TeamId", docData.Keys);
        Assert.Contains("CoachIds", docData.Keys);
        Assert.Contains("StartAt", docData.Keys);
        Assert.Contains("EndAt", docData.Keys);
        Assert.Contains("Location", docData.Keys);
        Assert.Contains("Capacity", docData.Keys);
        Assert.Contains("Status", docData.Keys);
    }

    [Fact]
    public void AttendanceRecord_Should_Serialize_With_PascalCase_Field_Names()
    {
        // Arrange
        var attendance = new AttendanceRecord
        {
            SessionId = "S001",
            MemberId = "M001",
            Status = "Present"
        };

        // Act
        var docData = new Dictionary<string, object>();
        var attendanceType = typeof(AttendanceRecord);
        
        foreach (var property in attendanceType.GetProperties())
        {
            var firestoreProperty = property.GetCustomAttribute<FirestorePropertyAttribute>();
            if (firestoreProperty != null)
            {
                var fieldName = firestoreProperty.Name ?? property.Name;
                var value = property.GetValue(attendance);
                if (value != null)
                {
                    docData[fieldName] = value;
                }
            }
        }

        // Assert
        Assert.Contains("SessionId", docData.Keys);
        Assert.Contains("MemberId", docData.Keys);
        Assert.Contains("Status", docData.Keys);
    }

    [Theory]
    [InlineData("Name", "name")]
    [InlineData("Phone", "phone")]
    [InlineData("Gender", "gender")]
    [InlineData("Status", "status")]
    [InlineData("TeamId", "teamId")]
    [InlineData("TeamIds", "teamIds")]
    [InlineData("Birthdate", "birthdate")]
    public void PascalCase_Should_Not_Match_CamelCase(string pascalCase, string camelCase)
    {
        // Assert that PascalCase and camelCase versions are different
        Assert.NotEqual(pascalCase, camelCase);
        Assert.True(char.IsUpper(pascalCase[0]));
        Assert.True(char.IsLower(camelCase[0]));
    }
}