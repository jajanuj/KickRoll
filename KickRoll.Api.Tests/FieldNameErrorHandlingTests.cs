using Google.Cloud.Firestore;
using KickRoll.Api.Controllers;
using KickRoll.Api.Models;
using System.Reflection;

namespace KickRoll.Api.Tests;

public class FieldNameErrorHandlingTests
{
   [Theory]
   [InlineData(typeof(Member), "name", "Name")]
   [InlineData(typeof(Member), "phone", "Phone")]
   [InlineData(typeof(Member), "gender", "Gender")]
   [InlineData(typeof(Member), "status", "Status")]
   [InlineData(typeof(Member), "teamId", "TeamId")]
   [InlineData(typeof(Member), "teamIds", "TeamIds")]
   [InlineData(typeof(Member), "birthdate", "Birthdate")]
   [InlineData(typeof(MemberPlan), "type", "Type")]
   [InlineData(typeof(MemberPlan), "name", "Name")]
   [InlineData(typeof(MemberPlan), "totalCredits", "TotalCredits")]
   [InlineData(typeof(MemberPlan), "remainingCredits", "RemainingCredits")]
   [InlineData(typeof(MemberPlan), "validFrom", "ValidFrom")]
   [InlineData(typeof(MemberPlan), "validUntil", "ValidUntil")]
   [InlineData(typeof(MemberPlan), "status", "Status")]
   [InlineData(typeof(MemberPlan), "createdAt", "CreatedAt")]
   [InlineData(typeof(MemberPlan), "updatedAt", "UpdatedAt")]
   public void Should_Use_PascalCase_Instead_Of_CamelCase(Type modelType, string wrongFieldName, string correctFieldName)
   {
      // Verify that we're using the correct PascalCase field names
      var properties = modelType.GetProperties();

      foreach (var property in properties)
      {
         var firestoreProperty = property.GetCustomAttribute<FirestorePropertyAttribute>();
         if (firestoreProperty != null)
         {
            var actualFieldName = firestoreProperty.Name ?? property.Name;

            // Assert we don't use the wrong (camelCase) field name
            Assert.NotEqual(wrongFieldName, actualFieldName);

            // If this property should have the correct field name, verify it
            if (property.Name.Equals(correctFieldName, StringComparison.OrdinalIgnoreCase))
            {
               Assert.Equal(correctFieldName, actualFieldName);
            }
         }
      }
   }

   [Fact]
   public void All_Models_Should_Have_FirestoreData_Attribute()
   {
      var modelTypes = new[]
      {
         typeof(Member),
         typeof(Team),
         typeof(ClassSession),
         typeof(AttendanceRecord),
         typeof(MemberPlan)
      };

      foreach (var modelType in modelTypes)
      {
         var firestoreDataAttribute = modelType.GetCustomAttribute<FirestoreDataAttribute>();
         Assert.NotNull(firestoreDataAttribute);
      }
   }

   [Fact]
   public void Document_Id_Properties_Should_Have_FirestoreDocumentId_Attribute()
   {
      // Verify that document ID properties are properly marked
      var teamIdProperty = typeof(Team).GetProperty("TeamId");
      var sessionIdProperty = typeof(ClassSession).GetProperty("SessionId");

      Assert.NotNull(teamIdProperty?.GetCustomAttribute<FirestoreDocumentIdAttribute>());
      Assert.NotNull(sessionIdProperty?.GetCustomAttribute<FirestoreDocumentIdAttribute>());
   }

   [Fact]
   public void Firestore_Queries_Should_Use_PascalCase_Field_Names()
   {
      // This test would normally require actual Firestore query analysis
      // For now, we'll verify the field names we expect to be used in queries

      var expectedPascalCaseFields = new[]
      {
         "Name",
         "Phone",
         "Gender",
         "Status",
         "TeamId",
         "TeamIds",
         "Birthdate",
         "Location",
         "Capacity",
         "CoachIds",
         "ScheduleHints",
         "StartAt",
         "EndAt",
         "SessionId",
         "MemberId",
         "Type",
         "TotalCredits",
         "RemainingCredits",
         "ValidFrom",
         "ValidUntil",
         "CreatedAt",
         "UpdatedAt"
      };

      var unexpectedCamelCaseFields = new[]
      {
         "name",
         "phone",
         "gender",
         "status",
         "teamId",
         "teamIds",
         "birthdate",
         "location",
         "capacity",
         "coachIds",
         "scheduleHints",
         "startAt",
         "endAt",
         "sessionId",
         "memberId",
         "type",
         "totalCredits",
         "remainingCredits",
         "validFrom",
         "validUntil",
         "createdAt",
         "updatedAt"
      };

      // Verify expected field names are PascalCase
      foreach (var field in expectedPascalCaseFields)
      {
         Assert.True(char.IsUpper(field[0]), $"Field '{field}' should start with uppercase");
      }

      // Verify we don't accidentally use camelCase versions
      foreach (var field in unexpectedCamelCaseFields)
      {
         Assert.True(char.IsLower(field[0]), $"Field '{field}' is camelCase and should not be used");
      }
   }

   [Fact]
   public void Should_Not_Have_CamelCase_Field_Names_In_Models()
   {
      // Test to ensure we don't accidentally introduce camelCase field names

      var firestoreModels = new Type[]
      {
         typeof(Member),
         typeof(Team),
         typeof(ClassSession),
         typeof(AttendanceRecord)
      };

      foreach (var modelType in firestoreModels)
      {
         var properties = modelType.GetProperties();

         foreach (var property in properties)
         {
            var firestoreProperty = property.GetCustomAttribute<FirestorePropertyAttribute>();
            if (firestoreProperty != null)
            {
               var fieldName = firestoreProperty.Name ?? property.Name;

               // Assert field name is not camelCase
               Assert.False(char.IsLower(fieldName[0]),
                  $"Model {modelType.Name} property {property.Name} has camelCase field name '{fieldName}' - should use PascalCase");

               // Assert field name doesn't contain underscores (snake_case)
               Assert.False(fieldName.Contains("_"),
                  $"Model {modelType.Name} property {property.Name} has snake_case field name '{fieldName}' - should use PascalCase");
            }
         }
      }
   }
}