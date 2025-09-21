namespace KickRoll.App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // 啟動頁設定為 MainPage，包在 NavigationPage 裡
        MainPage = new NavigationPage(new MainPage());
    }
}
