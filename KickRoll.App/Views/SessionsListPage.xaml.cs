using System.Net.Http.Json;

namespace KickRoll.App.Views;

public partial class SessionsListPage : ContentPage
{
   private readonly HttpClient _httpClient = new HttpClient
   {
      BaseAddress = new Uri("http://localhost:5112/") // ⚠️ 改成你的 API 埠號
   };

   public class SessionOption
   {
      public string SessionId { get; set; }
      public string TeamId { get; set; }
      public string TeamName { get; set; }
      public DateTime StartAt { get; set; }
      public DateTime EndAt { get; set; }
      public string Location { get; set; }
      public int Capacity { get; set; }
      public string Status { get; set; }
   }

   public SessionsListPage()
   {
      InitializeComponent();
      LoadSessions();
   }

   private async void LoadSessions()
   {
      try
      {
         var sessions = await _httpClient.GetFromJsonAsync<List<SessionOption>>("api/sessions/list");
         if (sessions != null)
         {
            SessionsCollection.ItemsSource = sessions;
            ResultLabel.Text = $"載入成功，共 {sessions.Count} 筆課表";
            ResultLabel.TextColor = Colors.Green;
         }
      }
      catch (Exception ex)
      {
         ResultLabel.TextColor = Colors.Red;
         ResultLabel.Text = $"❌ 載入課表失敗：{ex.Message}";
      }
   }

   private void OnReloadClicked(object sender, EventArgs e)
   {
      LoadSessions();
   }

   private async void OnSessionSelected(object sender, SelectionChangedEventArgs e)
   {
      if (e.CurrentSelection.FirstOrDefault() is SessionOption selectedSession)
      {
         await Navigation.PushAsync(new SessionDetailPage(selectedSession));
         ((CollectionView)sender).SelectedItem = null; // 清除選取狀態
      }
   }
}
