using System.Net.Http.Json;

namespace KickRoll.App.Views;

public partial class SessionDetailPage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5112/") // ⚠️ 改成你的 API 埠號
    };

    private readonly SessionsListPage.SessionOption _session;

    public class EnrollmentRequest
    {
        public string MemberId { get; set; } = default!;
    }

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

    private async void OnEnrollClicked(object sender, EventArgs e)
    {
        var memberId = MemberIdEntry.Text?.Trim();
        
        if (string.IsNullOrWhiteSpace(memberId))
        {
            EnrollmentResultLabel.TextColor = Colors.Red;
            EnrollmentResultLabel.Text = "⚠️ 請輸入會員 ID";
            return;
        }

        try
        {
            var request = new EnrollmentRequest { MemberId = memberId };
            var response = await _httpClient.PostAsJsonAsync($"api/sessions/{_session.SessionId}/enroll", request);
            
            if (response.IsSuccessStatusCode)
            {
                EnrollmentResultLabel.TextColor = Colors.Green;
                EnrollmentResultLabel.Text = "✅ 報名成功！";
                
                // Update the session info
                await RefreshSessionInfo();
                
                // Clear the input
                MemberIdEntry.Text = "";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                EnrollmentResultLabel.TextColor = Colors.Red;
                
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    if (errorContent.Contains("capacity_full"))
                    {
                        EnrollmentResultLabel.Text = "❌ 課程已滿員";
                    }
                    else if (errorContent.Contains("already_enrolled"))
                    {
                        EnrollmentResultLabel.Text = "❌ 該會員已報名此課程";
                    }
                    else
                    {
                        EnrollmentResultLabel.Text = "❌ 報名失敗，請檢查輸入";
                    }
                }
                else
                {
                    EnrollmentResultLabel.Text = $"❌ 報名失敗：{response.StatusCode}";
                }
            }
        }
        catch (Exception ex)
        {
            EnrollmentResultLabel.TextColor = Colors.Red;
            EnrollmentResultLabel.Text = $"❌ 例外：{ex.Message}";
        }
    }

    private async void OnCancelEnrollmentClicked(object sender, EventArgs e)
    {
        var memberId = MemberIdEntry.Text?.Trim();
        
        if (string.IsNullOrWhiteSpace(memberId))
        {
            EnrollmentResultLabel.TextColor = Colors.Red;
            EnrollmentResultLabel.Text = "⚠️ 請輸入會員 ID";
            return;
        }

        try
        {
            var request = new EnrollmentRequest { MemberId = memberId };
            var response = await _httpClient.PostAsJsonAsync($"api/sessions/{_session.SessionId}/cancel", request);
            
            if (response.IsSuccessStatusCode)
            {
                EnrollmentResultLabel.TextColor = Colors.Orange;
                EnrollmentResultLabel.Text = "✅ 取消報名成功！";
                
                // Update the session info
                await RefreshSessionInfo();
                
                // Clear the input
                MemberIdEntry.Text = "";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                EnrollmentResultLabel.TextColor = Colors.Red;
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    EnrollmentResultLabel.Text = "❌ 找不到該會員的報名紀錄";
                }
                else
                {
                    EnrollmentResultLabel.Text = $"❌ 取消報名失敗：{response.StatusCode}";
                }
            }
        }
        catch (Exception ex)
        {
            EnrollmentResultLabel.TextColor = Colors.Red;
            EnrollmentResultLabel.Text = $"❌ 例外：{ex.Message}";
        }
    }

    private async Task RefreshSessionInfo()
    {
        try
        {
            // In a real implementation, we might want to refresh the session data
            // from the server to get updated enrollment counts
            // For now, we'll just show that the operation was successful
        }
        catch (Exception ex)
        {
            // Log the error but don't show it to user as this is secondary
            System.Diagnostics.Debug.WriteLine($"Failed to refresh session info: {ex.Message}");
        }
    }
}
