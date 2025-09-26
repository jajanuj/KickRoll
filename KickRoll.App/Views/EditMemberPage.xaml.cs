using System.Net.Http.Json;
using System.Text.Json;

namespace KickRoll.App.Views;

public partial class EditMemberPage : ContentPage
{
   #region Fields

   private readonly HttpClient _httpClient = new HttpClient
   {
      BaseAddress = new Uri("http://localhost:5112/") // ⚠️ 改成你的 API 埠號
   };

   private MemberDetail _member; // 宣告 _member 變數
   private string? _memberId;

   // 假設你會從 API 得到所有隊伍的資料
   private List<Team> TeamOptions = new List<Team>();

   #endregion

   #region Constructors

   public EditMemberPage(MemberDetail member)
   {
      InitializeComponent();
      _member = member;
      _memberId = member.MemberId;

      NameEntry.Text = member.Name;
      PhoneEntry.Text = member.Phone;
      BirthdatePicker.Date = member.Birthdate;

      GenderPicker.SelectedItem = member.Gender;
      StatusPicker.SelectedItem = member.Status;

      _ = LoadTeams();
   }

   #endregion

   #region Private Methods

   private async Task LoadTeams()
   {
      // 假設你會透過 API 取得隊伍清單
      TeamOptions = await _httpClient.GetFromJsonAsync<List<Team>>("api/teams/list");

      // 假設 API 傳回來的成員有對應的 TeamIds
      foreach (var team in TeamOptions)
      {
         team.IsSelected = _member.TeamId == team.TeamId;

         if (team.IsSelected)
         {
            break;
         }
         //team.IsSelected = _member.TeamIds.Contains(team.TeamId);
      }

      TeamsCollection.ItemsSource = TeamOptions;
   }

   private async void OnSaveMemberClicked(object sender, EventArgs e)
   {
      try
      {
         var selectedTeams = TeamsCollection.ItemsSource.Cast<Team>()
            .Where(t => t.IsSelected)
            .Select(t => t.TeamId)
            .ToList();

         var updatedMember = new
         {
            MemberId = _memberId,
            Name = NameEntry.Text,
            Phone = PhoneEntry.Text,
            Gender = GenderPicker.SelectedItem?.ToString() ?? "",
            Status = StatusPicker.SelectedItem?.ToString() ?? "active",
            TeamId = selectedTeams.FirstOrDefault() ?? "",
            TeamIds = selectedTeams,
            Birthdate = DateTime.SpecifyKind(BirthdatePicker.Date, DateTimeKind.Utc)
         };

         var response = await _httpClient.PutAsJsonAsync($"api/members/{_memberId}", updatedMember);

         if (response.IsSuccessStatusCode)
         {
            await DisplayAlert("成功", "成員已更新", "OK");
            await Navigation.PopAsync();
         }
         else
         {
            var errorContent = await response.Content.ReadAsStringAsync();
            try
            {
               // Try to parse JSON error response
               var errorResponse = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(errorContent);
               
               if (errorResponse.TryGetProperty("error", out var errorMessage))
               {
                  await DisplayAlert("錯誤", errorMessage.GetString(), "OK");
               }
               else
               {
                  await DisplayAlert("錯誤", $"更新失敗：{response.StatusCode}", "OK");
               }
            }
            catch
            {
               // If JSON parsing fails, show generic error
               await DisplayAlert("錯誤", $"更新失敗：{response.StatusCode}", "OK");
            }
         }
      }
      catch (Exception ex)
      {
         await DisplayAlert("錯誤", ex.Message, "OK");
      }
   }

   private async void OnManagePlansClicked(object sender, EventArgs e)
   {
      await Navigation.PushAsync(new MemberPlansPage(_memberId!, _member.Name!));
   }

   #endregion
}

public class Team
{
   #region Properties

   public required string TeamId { get; set; }
   public required string Name { get; set; }
   public bool IsSelected { get; set; }

   #endregion
}