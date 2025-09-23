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
            StatusLabel.Text = "載入課程資訊中...";
            StatusLabel.TextColor = Colors.Blue;

            // 清空現有資料
            _enrolledCourses.Clear();
            _availableCourses.Clear();

            // 載入成員已加入的課程
            var enrolledCoursesResponse = await _httpClient.GetAsync($"api/courses/members/{_member.MemberId}");
            if (enrolledCoursesResponse.IsSuccessStatusCode)
            {
                var enrolledCourses = await enrolledCoursesResponse.Content.ReadFromJsonAsync<List<CourseInfo>>();
                if (enrolledCourses != null)
                {
                    foreach (var course in enrolledCourses)
                    {
                        _enrolledCourses.Add(course);
                    }
                }
                StatusLabel.Text = $"已載入 {_enrolledCourses.Count} 個已加入課程";
            }
            else
            {
                var errorContent = await enrolledCoursesResponse.Content.ReadAsStringAsync();
                StatusLabel.Text = $"載入已加入課程失敗：HTTP {enrolledCoursesResponse.StatusCode} - {errorContent}";
                StatusLabel.TextColor = Colors.Orange;
            }

            // 載入所有可用課程
            var allCoursesResponse = await _httpClient.GetAsync("api/courses/list");
            if (allCoursesResponse.IsSuccessStatusCode)
            {
                var responseContent = await allCoursesResponse.Content.ReadAsStringAsync();
                StatusLabel.Text += $"\nAPI 回應內容: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}...";
                
                var allCourses = await allCoursesResponse.Content.ReadFromJsonAsync<List<CourseInfo>>();
                
                if (allCourses != null && allCourses.Any())
                {
                    var enrolledCourseIds = _enrolledCourses.Select(c => c.CourseId).ToHashSet();
                    
                    int totalActive = 0;
                    foreach (var course in allCourses)
                    {
                        // Log course details for debugging
                        StatusLabel.Text += $"\n課程: {course.Name} (狀態: {course.Status}, ID: {course.CourseId})";
                        
                        if (course.Status == "Active")
                        {
                            totalActive++;
                        }
                        
                        // 篩選條件：
                        // 1. 成員尚未加入此課程
                        // 2. 課程狀態為 "Active"
                        if (!enrolledCourseIds.Contains(course.CourseId) && course.Status == "Active")
                        {
                            _availableCourses.Add(course);
                        }
                    }
                    
                    StatusLabel.Text = $"載入完成：共 {allCourses.Count} 個課程，{totalActive} 個狀態 Active，{_enrolledCourses.Count} 個已加入，{_availableCourses.Count} 個可加入";
                    StatusLabel.TextColor = Colors.Green;
                }
                else
                {
                    StatusLabel.Text = "API 回應為空或無課程資料";
                    StatusLabel.TextColor = Colors.Orange;
                }
            }
            else
            {
                var errorContent = await allCoursesResponse.Content.ReadAsStringAsync();
                StatusLabel.Text = $"無法連接課程服務：HTTP {allCoursesResponse.StatusCode} - {errorContent}";
                StatusLabel.TextColor = Colors.Red;
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"載入課程資訊失敗：{ex.Message}\n堆疊: {ex.StackTrace}";
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