using System.Net.Http.Json;

namespace KickRoll.App.Views;

public partial class SessionDetailPage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5112/") // ⚠️ 改成你的 API 埠號
    };

    private readonly SessionsListPage.SessionOption _session;

    public class MemberOption
    {
        public string MemberId { get; set; }
        public string Name { get; set; }
        public bool IsPresent { get; set; } // 綁定 CheckBox
    }

    public SessionDetailPage(SessionsListPage.SessionOption session)
    {
        InitializeComponent();
        _session = session;
        BindingContext = session;
        LoadMembers(session.TeamId);
    }

    private async void LoadMembers(string teamId)
    {
        try
        {
            var members = await _httpClient.GetFromJsonAsync<List<MemberOption>>($"api/members/byTeam/{teamId}");
            if (members != null && members.Count > 0)
            {
                MembersCollection.ItemsSource = members;
                MembersCollection.IsVisible = true;
                NoMembersLabel.IsVisible = false;
            }
            else
            {
                MembersCollection.ItemsSource = null;
                MembersCollection.IsVisible = false;
                NoMembersLabel.IsVisible = true; // 顯示「尚無成員」
            }
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"❌ 載入成員失敗：{ex.Message}";
        }
    }

    private async void OnSubmitAttendanceClicked(object sender, EventArgs e)
    {
        if (MembersCollection.ItemsSource is List<MemberOption> members)
        {
            var records = members.Select(m => new
            {
                SessionId = _session.SessionId,
                MemberId = m.MemberId,
                Status = m.IsPresent ? "Present" : "Absent"
            }).ToList();

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/attendance/submit", records);
                if (response.IsSuccessStatusCode)
                {
                    ResultLabel.TextColor = Colors.Green;
                    ResultLabel.Text = "✅ 點名已提交";
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
}
