using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace KickRoll.App.Views;

public partial class MembersListPage : ContentPage
{
   private readonly HttpClient _httpClient = new HttpClient
   {
      BaseAddress = new Uri("http://localhost:5112/") // ⚠️ 改成你的 API 埠號
   };

   public class MemberOption
   {
      [JsonPropertyName("memberId")]
      public string MemberId { get; set; }

      [JsonPropertyName("name")]
      public string Name { get; set; }

      [JsonPropertyName("status")]
      public string Status { get; set; }

      // 新增 TeamNames 屬性修復綁定警告
      [JsonPropertyName("teamNames")]
      public string TeamNames { get; set; } = "(未分配隊伍)";
   }

   public MembersListPage()
   {
      InitializeComponent();
      LoadMembersAsync();
   }

   private async Task LoadMembersAsync()
   {
      try
      {
         var members = await _httpClient.GetFromJsonAsync<List<MemberOption>>("api/members/list");
         if (members != null)
         {
            MembersCollection.ItemsSource = members;
         }
      }
      catch (Exception ex)
      {
         ResultLabel.Text = $"❌ 載入成員失敗：{ex.Message}";
      }
   }

   private async void OnEditMemberClicked(object sender, EventArgs e)
   {
      if (sender is Button button && button.CommandParameter is MemberOption selected)
      {
         try
         {
            var member = await _httpClient.GetFromJsonAsync<MemberDetail>($"api/members/{selected.MemberId}");

            if (member != null)
            {
               await Navigation.PushAsync(new EditMemberPage(member));
            }
         }
         catch (Exception ex)
         {
            await DisplayAlert("錯誤", $"無法載入成員資料：{ex.Message}", "OK");
         }
      }
   }

   private async void OnViewEnrollmentsClicked(object sender, EventArgs e)
   {
      if (sender is Button button && button.CommandParameter is MemberOption selected)
      {
         await Navigation.PushAsync(new MemberEnrollmentsPage(selected.MemberId, selected.Name ?? "未知會員"));
      }
   }

   private async void OnManagePlansClicked(object sender, EventArgs e)
   {
      if (sender is Button button && button.CommandParameter is MemberOption selected)
      {
         await Navigation.PushAsync(new MemberPlansPage(selected.MemberId, selected.Name ?? "未知會員"));
      }
   }

   protected override async void OnAppearing()
   {
      base.OnAppearing();
      await LoadMembersAsync(); // ✅ 每次回到頁面都重新抓 Firestore
   }

   private async void OnAddMemberFabClicked(object sender, EventArgs e)
   {
      await Navigation.PushAsync(new AddMemberPage());
   }
}

public class MemberDetail
{
   public string? MemberId { get; set; }
   public string? Name { get; set; }
   public string? Phone { get; set; }
   public string? Gender { get; set; }
   public DateTime Birthdate { get; set; }
   public string? Status { get; set; }
   public string? TeamId { get; set; }
   public List<string>? TeamIds { get; set; }
}

