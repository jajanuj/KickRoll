using System.Net.Http.Json;

namespace KickRoll.App.Views;

public partial class AddTeamPage : ContentPage
{
   #region Fields

   private readonly HttpClient _httpClient = new HttpClient
   {
      BaseAddress = new Uri("http://localhost:5112/")
   };

   #endregion

   #region Constructors

   public AddTeamPage()
   {
      InitializeComponent();
   }

   #endregion

   #region Private Methods

   private async void OnCreateTeamClicked(object sender, EventArgs e)
   {
      var team = new
      {
         Name = TeamNameEntry.Text,
         Location = LocationEntry.Text,
         Capacity = int.TryParse(CapacityEntry.Text, out var c) ? c : 0,
         ScheduleHints = ScheduleHintsEditor.Text
      };

      try
      {
         var response = await _httpClient.PostAsJsonAsync("api/teams", team);
         if (response.IsSuccessStatusCode)
         {
            ResultLabel.TextColor = Colors.Green;
            ResultLabel.Text = "✅ 隊別建立成功";
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

   #endregion
}