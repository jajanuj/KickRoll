using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace KickRoll.App.Views;

public partial class MemberDetailPage : ContentPage
{
    #region Fields

    private readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5112/") // ⚠️ 改成你的 API 埠號
    };

    private readonly MemberDetail _member;
    private readonly ObservableCollection<CourseInfo> _enrolledCourses = new();
    private readonly ObservableCollection<CourseInfo> _availableCourses = new();

    #endregion

    #region Properties

    public ObservableCollection<CourseInfo> EnrolledCourses => _enrolledCourses;
    public ObservableCollection<CourseInfo> AvailableCourses => _availableCourses;
    public MemberDetail Member => _member;

    #endregion

    #region Constructor

    public MemberDetailPage(MemberDetail member)
    {
        InitializeComponent();
        _member = member;
        BindingContext = this;
        
        // 設置成員基本資訊的綁定源
        this.SetBinding(TitleProperty, new Binding("Name", source: _member, stringFormat: "{0} - 成員詳情"));
        
        // 載入課程資訊
        _ = LoadCoursesAsync();
    }

    #endregion

    #region Private Methods

    private async Task LoadCoursesAsync()
    {
        try
        {
            // 清空現有資料
            _enrolledCourses.Clear();
            _availableCourses.Clear();

            // 載入成員已加入的課程
            var enrolledCourses = await _httpClient.GetFromJsonAsync<List<CourseInfo>>($"api/courses/members/{_member.MemberId}");
            if (enrolledCourses != null)
            {
                foreach (var course in enrolledCourses)
                {
                    _enrolledCourses.Add(course);
                }
            }

            // 載入所有可用課程
            var allCourses = await _httpClient.GetFromJsonAsync<List<CourseInfo>>("api/courses/list");
            if (allCourses != null)
            {
                var enrolledCourseIds = _enrolledCourses.Select(c => c.CourseId).ToHashSet();
                
                foreach (var course in allCourses)
                {
                    if (!enrolledCourseIds.Contains(course.CourseId) && course.Status == "Active")
                    {
                        _availableCourses.Add(course);
                    }
                }
            }

            StatusLabel.Text = "";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"載入課程資訊失敗：{ex.Message}";
            StatusLabel.TextColor = Colors.Red;
        }
    }

    private async void OnJoinCourseClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string courseId)
        {
            try
            {
                button.IsEnabled = false;
                button.Text = "加入中...";

                var response = await _httpClient.PostAsync($"api/courses/{courseId}/members/{_member.MemberId}", null);

                if (response.IsSuccessStatusCode)
                {
                    StatusLabel.Text = "成功加入課程！";
                    StatusLabel.TextColor = Colors.Green;
                    
                    // 重新載入課程列表
                    await LoadCoursesAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    StatusLabel.Text = $"加入失敗：{response.StatusCode} - {errorContent}";
                    StatusLabel.TextColor = Colors.Red;
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"加入課程時發生錯誤：{ex.Message}";
                StatusLabel.TextColor = Colors.Red;
            }
            finally
            {
                button.IsEnabled = true;
                button.Text = "加入";
            }
        }
    }

    private async void OnEditMemberClicked(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new EditMemberPage(_member));
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"開啟編輯頁面失敗：{ex.Message}";
            StatusLabel.TextColor = Colors.Red;
        }
    }

    #endregion

    #region Override Methods

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // 每次頁面出現時重新載入課程資訊
        await LoadCoursesAsync();
    }

    #endregion
}

#region Data Models

public class CourseInfo
{
    public string CourseId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string InstructorId { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime JoinedAt { get; set; }
}

#endregion