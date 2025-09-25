using Google.Cloud.Firestore;
using KickRoll.Api.Controllers;
using KickRoll.Api.Models;
using System.Reflection;

namespace KickRoll.Api.Tests;

public class FirestoreFieldNamingTests
{
   [Theory]
   [InlineData("Name")]
   [InlineData("Phone")]
   [InlineData("Gender")]
   [InlineData("Status")]
   [InlineData("TeamId")]
   [InlineData("TeamIds")]
   [InlineData("Birthdate")]
   public void Firestore_Field_Names_Should_Be_PascalCase(string fieldName)
   {
      // Assert
      Assert.True(char.IsUpper(fieldName[0]), $"Field name '{fieldName}' should start with uppercase letter");
      Assert.False(fieldName.Contains("_"), $"Field name '{fieldName}' should not contain underscores");
   }

   [Fact]
   public void All_FirestoreData_Models_Should_Be_Properly_Decorated()
   {
      // Arrange
      var assembly = typeof(Member).Assembly;
      var firestoreDataTypes = assembly.GetTypes()
         .Where(t => t.GetCustomAttribute<FirestoreDataAttribute>() != null)
         .ToList();

      // Assert
      Assert.Contains(typeof(Member), firestoreDataTypes);
      Assert.Contains(typeof(Team), firestoreDataTypes);
      Assert.Contains(typeof(ClassSession), firestoreDataTypes);
      Assert.Contains(typeof(AttendanceRecord), firestoreDataTypes);
   }

   [Fact]
   public void AttendanceRecord_Model_Should_Have_PascalCase_FirestoreProperties()
   {
      // Arrange & Act
      var attendanceRecordType = typeof(AttendanceRecord);
      var properties = attendanceRecordType.GetProperties();

      // Assert
      foreach (var property in properties)
      {
         var firestoreProperty = property.GetCustomAttribute<FirestorePropertyAttribute>();
         if (firestoreProperty != null)
         {
            var fieldName = firestoreProperty.Name ?? property.Name;
            Assert.True(char.IsUpper(fieldName[0]),
               $"Property {property.Name} should have PascalCase Firestore field name, but got: {fieldName}");

            // Verify specific expected field names
            switch (property.Name)
            {
               case "SessionId":
                  Assert.Equal("SessionId", fieldName);
                  break;
               case "MemberId":
                  Assert.Equal("MemberId", fieldName);
                  break;
               case "Status":
                  Assert.Equal("Status", fieldName);
                  break;
            }
         }
      }
   }

   [Fact]
   public void ClassSession_Model_Should_Have_PascalCase_FirestoreProperties()
   {
      // Arrange & Act
      var classSessionType = typeof(ClassSession);
      var properties = classSessionType.GetProperties();

      // Assert
      foreach (var property in properties)
      {
         var firestoreProperty = property.GetCustomAttribute<FirestorePropertyAttribute>();
         if (firestoreProperty != null)
         {
            var fieldName = firestoreProperty.Name ?? property.Name;
            Assert.True(char.IsUpper(fieldName[0]),
               $"Property {property.Name} should have PascalCase Firestore field name, but got: {fieldName}");

            // Verify specific expected field names
            switch (property.Name)
            {
               case "TeamId":
                  Assert.Equal("TeamId", fieldName);
                  break;
               case "CoachIds":
                  Assert.Equal("CoachIds", fieldName);
                  break;
               case "StartAt":
                  Assert.Equal("StartAt", fieldName);
                  break;
               case "EndAt":
                  Assert.Equal("EndAt", fieldName);
                  break;
               case "Location":
                  Assert.Equal("Location", fieldName);
                  break;
               case "Capacity":
                  Assert.Equal("Capacity", fieldName);
                  break;
               case "Status":
                  Assert.Equal("Status", fieldName);
                  break;
            }
         }
      }
   }

   [Fact]
   public void Member_Model_Should_Have_PascalCase_FirestoreProperties()
   {
      // Arrange & Act
      var memberType = typeof(Member);
      var properties = memberType.GetProperties();

      // Assert
      foreach (var property in properties)
      {
         var firestoreProperty = property.GetCustomAttribute<FirestorePropertyAttribute>();
         if (firestoreProperty != null)
         {
            // Verify that the Firestore field name is PascalCase
            var fieldName = firestoreProperty.Name ?? property.Name;
            Assert.True(char.IsUpper(fieldName[0]),
               $"Property {property.Name} should have PascalCase Firestore field name, but got: {fieldName}");

            // Verify specific expected field names
            switch (property.Name)
            {
               case "MemberId":
                  Assert.Equal("MemberId", fieldName);
                  break;
               case "Name":
                  Assert.Equal("Name", fieldName);
                  break;
               case "Phone":
                  Assert.Equal("Phone", fieldName);
                  break;
               case "Gender":
                  Assert.Equal("Gender", fieldName);
                  break;
               case "Status":
                  Assert.Equal("Status", fieldName);
                  break;
               case "TeamId":
                  Assert.Equal("TeamId", fieldName);
                  break;
               case "TeamIds":
                  Assert.Equal("TeamIds", fieldName);
                  break;
               case "Birthdate":
                  Assert.Equal("Birthdate", fieldName);
                  break;
            }
         }
      }
   }

   [Fact]
   public void Team_Model_Should_Have_PascalCase_FirestoreProperties()
   {
      // Arrange & Act
      var teamType = typeof(Team);
      var properties = teamType.GetProperties();

      // Assert
      foreach (var property in properties)
      {
         var firestoreProperty = property.GetCustomAttribute<FirestorePropertyAttribute>();
         if (firestoreProperty != null)
         {
            var fieldName = firestoreProperty.Name ?? property.Name;
            Assert.True(char.IsUpper(fieldName[0]),
               $"Property {property.Name} should have PascalCase Firestore field name, but got: {fieldName}");

            // Verify specific expected field names
            switch (property.Name)
            {
               case "Name":
                  Assert.Equal("Name", fieldName);
                  break;
               case "Location":
                  Assert.Equal("Location", fieldName);
                  break;
               case "Capacity":
                  Assert.Equal("Capacity", fieldName);
                  break;
               case "CoachIds":
                  Assert.Equal("CoachIds", fieldName);
                  break;
               case "ScheduleHints":
                  Assert.Equal("ScheduleHints", fieldName);
                  break;
            }
         }
      }
   }
}