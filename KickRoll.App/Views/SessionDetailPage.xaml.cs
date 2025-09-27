using System.Net.Http.Json;

namespace KickRoll.App.Views;

public partial class SessionDetailPage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5112/") // ⚠️ 改成你的 API 埠號
    };

    private readonly SessionsListPage.SessionOption _session;
    private List<MemberDropdownOption> _allMembers = new List<MemberDropdownOption>();
    private List<MemberDropdownOption> _filteredMembers = new List<MemberDropdownOption>();
    private string _selectedMemberId = string.Empty;
    private List<EnrolledMember> _enrolledMembers = new List<EnrolledMember>();
    private List<EnrolledMember> _filteredEnrolledMembers = new List<EnrolledMember>();

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

    public class MemberDropdownOption
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class EnrolledMember
    {
        public string MemberId { get; set; }
        public string Name { get; set; }
        public DateTime EnrolledAt { get; set; }
    }

    public class EnrolledMembersResponse
    {
        public string SessionId { get; set; }
        public int TotalCount { get; set; }
        public List<EnrolledMember> Members { get; set; } = new List<EnrolledMember>();
    }

    public SessionDetailPage(SessionsListPage.SessionOption session)
    {
        InitializeComponent();
        _session = session;
        BindingContext = session;
        LoadMembers(session.TeamId);
        LoadMembersForDropdown();
        LoadEnrolledMembers();
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

    private async void LoadMembersForDropdown()
    {
        try
        {
            LoadingLabel.IsVisible = true;
            ErrorPanel.IsVisible = false;
            MemberPicker.IsEnabled = false;
            SearchEntry.IsEnabled = false;
            
            var members = await _httpClient.GetFromJsonAsync<List<MemberDropdownOption>>("api/members/dropdown");
            if (members != null)
            {
                _allMembers = members;
                _filteredMembers = new List<MemberDropdownOption>(_allMembers);
                UpdatePickerItemsSource();
                MemberPicker.IsEnabled = true;
                SearchEntry.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"❌ 載入會員名單失敗：{ex.Message}";
            ErrorPanel.IsVisible = true;
        }
        finally
        {
            LoadingLabel.IsVisible = false;
        }
    }

    private void OnRetryClicked(object sender, EventArgs e)
    {
        LoadMembersForDropdown();
    }

    private async void LoadEnrolledMembers()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<EnrolledMembersResponse>($"api/sessions/{_session.SessionId}/enrolled-members");
            if (response != null)
            {
                _enrolledMembers = response.Members;
                _filteredEnrolledMembers = new List<EnrolledMember>(_enrolledMembers);
                UpdateEnrolledMembersDisplay();
            }
        }
        catch (Exception ex)
        {
            // Log error but don't show to user as this is not critical
            System.Diagnostics.Debug.WriteLine($"❌ 載入已報名名單失敗：{ex.Message}");
        }
    }

    private void UpdateEnrolledMembersDisplay()
    {
        EnrolledCountLabel.Text = $"已報名：{_enrolledMembers.Count} 人";
        
        if (_enrolledMembers.Count == 0)
        {
            EnrolledMembersCollection.IsVisible = false;
            NoEnrolledMembersLabel.IsVisible = true;
            EnrolledSearchEntry.IsVisible = false;
        }
        else
        {
            EnrolledMembersCollection.ItemsSource = _filteredEnrolledMembers;
            EnrolledMembersCollection.IsVisible = true;
            NoEnrolledMembersLabel.IsVisible = false;
            
            // Show search entry for large lists
            EnrolledSearchEntry.IsVisible = _enrolledMembers.Count > 10;
        }
    }

    private void OnRefreshEnrolledClicked(object sender, EventArgs e)
    {
        LoadEnrolledMembers();
    }

    private void OnEnrolledSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue?.Trim().ToLower() ?? string.Empty;
        
        if (string.IsNullOrEmpty(searchText))
        {
            _filteredEnrolledMembers = new List<EnrolledMember>(_enrolledMembers);
        }
        else
        {
            _filteredEnrolledMembers = _enrolledMembers
                .Where(m => m.Name.ToLower().Contains(searchText))
                .ToList();
        }
        
        EnrolledMembersCollection.ItemsSource = _filteredEnrolledMembers;
    }

    private void OnEnrolledMemberClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string memberId)
        {
            // Auto-select the member for cancellation
            SetSelectedMember(memberId);
            
            // Show feedback to user
            EnrollmentResultLabel.TextColor = Colors.Blue;
            var memberName = _enrolledMembers.FirstOrDefault(m => m.MemberId == memberId)?.Name ?? "Unknown";
            EnrollmentResultLabel.Text = $"已選擇 {memberName} 進行取消報名";
        }
    }

    private void SetSelectedMember(string memberId)
    {
        if (_allMembers != null && !string.IsNullOrEmpty(memberId))
        {
            var member = _filteredMembers.FirstOrDefault(m => m.Id == memberId);
            if (member != null)
            {
                MemberPicker.SelectedItem = member;
                _selectedMemberId = memberId;
            }
            else
            {
                // Member is not in current filtered list, clear search to show all
                SearchEntry.Text = string.Empty;
                _filteredMembers = new List<MemberDropdownOption>(_allMembers);
                UpdatePickerItemsSource();
                
                member = _filteredMembers.FirstOrDefault(m => m.Id == memberId);
                if (member != null)
                {
                    MemberPicker.SelectedItem = member;
                    _selectedMemberId = memberId;
                }
            }
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue?.Trim().ToLower() ?? string.Empty;
        
        if (string.IsNullOrEmpty(searchText))
        {
            _filteredMembers = new List<MemberDropdownOption>(_allMembers);
        }
        else
        {
            _filteredMembers = _allMembers.Where(m => 
                m.Name.ToLower().Contains(searchText)).ToList();
        }
        
        UpdatePickerItemsSource();
        
        // Clear selection if current selection is not in filtered results
        if (_selectedMemberId != null && !_filteredMembers.Any(m => m.Id == _selectedMemberId))
        {
            MemberPicker.SelectedItem = null;
            _selectedMemberId = string.Empty;
        }
    }

    private void UpdatePickerItemsSource()
    {
        MemberPicker.ItemsSource = null;
        MemberPicker.ItemsSource = _filteredMembers;
    }

    private void OnMemberPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        if (picker?.SelectedItem is MemberDropdownOption selected)
        {
            _selectedMemberId = selected.Id;
        }
        else
        {
            _selectedMemberId = string.Empty;
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
        var memberId = _selectedMemberId?.Trim();
        
        if (string.IsNullOrWhiteSpace(memberId))
        {
            EnrollmentResultLabel.TextColor = Colors.Red;
            EnrollmentResultLabel.Text = "⚠️ 請選擇會員";
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
                
                // Clear the selection
                MemberPicker.SelectedItem = null;
                _selectedMemberId = string.Empty;
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
        var memberId = _selectedMemberId?.Trim();
        
        if (string.IsNullOrWhiteSpace(memberId))
        {
            EnrollmentResultLabel.TextColor = Colors.Red;
            EnrollmentResultLabel.Text = "⚠️ 請選擇會員";
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
                
                // Clear the selection
                MemberPicker.SelectedItem = null;
                _selectedMemberId = string.Empty;
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
            // Refresh the enrolled members list to show updated data
            await Task.Delay(500); // Small delay to ensure server has processed the change
            LoadEnrolledMembers();
        }
        catch (Exception ex)
        {
            // Log the error but don't show it to user as this is secondary
            System.Diagnostics.Debug.WriteLine($"Failed to refresh session info: {ex.Message}");
        }
    }
}
