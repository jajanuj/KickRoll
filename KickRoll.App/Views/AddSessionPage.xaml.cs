using System.Net.Http.Json;

namespace KickRoll.App.Views;

public partial class AddSessionPage : ContentPage
{
   private readonly HttpClient _httpClient = new HttpClient
   {
      BaseAddress = new Uri("http://localhost:5112/") // ⚠️ 改成你的 API 埠號
   };

   public class TeamOption
   {
      public string TeamId { get; set; }
      public string Name { get; set; }
   }

   public AddSessionPage()
   {
      InitializeComponent();

      StartTimePicker.PropertyChanged += StartTimePicker_PropertyChanged;

      LoadTeams();
   }

   private async void LoadTeams()
   {
      try
      {
         var teams = await _httpClient.GetFromJsonAsync<List<TeamOption>>("api/teams/list");
         if (teams != null)
         {
            TeamPicker.ItemsSource = teams;
         }
      }
      catch (Exception ex)
      {
         ResultLabel.TextColor = Colors.Red;
         ResultLabel.Text = $"❌ 載入隊伍失敗：{ex.Message}";
      }
   }

   private void StartTimePicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
   {
      if (e.PropertyName == nameof(StartTimePicker.Time))
      {
         EndTimePicker.Time = StartTimePicker.Time.Add(TimeSpan.FromHours(2));
      }
   }

   private async void OnCreateSessionClicked(object sender, EventArgs e)
   {
      if (TeamPicker.SelectedItem is not TeamOption selectedTeam)
      {
         ResultLabel.TextColor = Colors.Red;
         ResultLabel.Text = "❌ 請先選擇隊伍";
         return;
      }

      DateTime startAt = (StartDatePicker.Date + StartTimePicker.Time).ToUniversalTime();
      DateTime endAt = (StartDatePicker.Date + EndTimePicker.Time).ToUniversalTime();

      var session = new
      {
         TeamId = selectedTeam.TeamId,
         StartAt = startAt,
         EndAt = endAt,
         Location = LocationEntry.Text,
         Capacity = int.TryParse(CapacityEntry.Text, out var c) ? c : 0,
         CoachIds = new string[] { },
         Status = "Scheduled"
      };

      try
      {
         var response = await _httpClient.PostAsJsonAsync("api/sessions", session);
         if (response.IsSuccessStatusCode)
         {
            ResultLabel.TextColor = Colors.Green;
            ResultLabel.Text = "✅ 課表建立成功";
         }
         else
         {
            ResultLabel.TextColor = Colors.Red;
            ResultLabel.Text = $"❌ 錯誤：{response.StatusCode}";
         }
      }
      catch (Exception ex)
      {
         ResultLabel.TextColor = Colors.Red;
         ResultLabel.Text = $"❌ 例外：{ex.Message}";
      }
   }
}
