using System.Net.Http.Json;

namespace KickRoll.App.Views;

public partial class EditMemberPage : ContentPage
{
   private string _memberId;

   // 假設你會從 API 得到所有隊伍的資料
   private List<Team> TeamOptions = new List<Team>();

   private readonly HttpClient _httpClient = new HttpClient
   {
      BaseAddress = new Uri("http://localhost:5112/") // ⚠️ 改成你的 API 埠號
   };
   private MemberDetail _member; // 宣告 _member 變數

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

      LoadTeams();
   }

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
            ResultLabel.Text = "更新失敗：" + response.StatusCode;
         }
      }
      catch (Exception ex)
      {
         ResultLabel.Text = "錯誤：" + ex.Message;
      }
   }
}

public class Team
{
   public string TeamId { get; set; }
   public string Name { get; set; }
   public bool IsSelected { get; set; }
}
