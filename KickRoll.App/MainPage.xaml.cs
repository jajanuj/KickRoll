using KickRoll.App.Views;

namespace KickRoll.App;

public partial class MainPage : ContentPage
{
   public MainPage()
   {
      InitializeComponent();
   }

   private async void OnAddTeamClicked(object sender, EventArgs e)
   {
      await Navigation.PushAsync(new AddTeamPage());
   }

   private async void OnAddSessionClicked(object sender, EventArgs e)
   {
      await Navigation.PushAsync(new AddSessionPage());
   }

   private async void OnSessionsListClicked(object sender, EventArgs e)
   {
      await Navigation.PushAsync(new SessionsListPage());
   }

   private async void OnMembersListClicked(object sender, EventArgs e)
   {
      await Navigation.PushAsync(new MembersListPage());
   }
}