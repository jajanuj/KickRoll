using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KickRoll.App.Views;

// API Response DTOs - must match the API response structure
public class SessionEnrollmentResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;
    
    [JsonPropertyName("courseId")]
    public string CourseId { get; set; } = default!;
    
    [JsonPropertyName("teamId")]
    public string TeamId { get; set; } = default!;
    
    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }
    
    [JsonPropertyName("endTime")]
    public DateTime EndTime { get; set; }
    
    [JsonPropertyName("capacity")]
    public int Capacity { get; set; }
    
    [JsonPropertyName("enrolledCount")]
    public int EnrolledCount { get; set; }
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

public class MemberEnrollmentsResponse
{
    [JsonPropertyName("enrollmentId")]
    public string EnrollmentId { get; set; } = default!;
    
    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = default!;
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("session")]
    public SessionEnrollmentResponse Session { get; set; } = default!;
    
    // Additional properties for UI display
    public Color StatusColor => Status switch
    {
        "enrolled" => Colors.Green,
        "cancelled" => Colors.Orange,
        _ => Colors.Gray
    };
}

public partial class MemberEnrollmentsPage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5112/"), // ⚠️ 改成你的 API 埠號
        Timeout = TimeSpan.FromSeconds(30) // Add timeout to prevent hanging
    };

    private readonly string _memberId;
    private readonly string _memberName;

    public MemberEnrollmentsPage(string memberId, string memberName)
    {
        InitializeComponent();
        _memberId = memberId;
        _memberName = memberName;

        MemberNameLabel.Text = memberName;
        MemberIdLabel.Text = $"ID: {memberId}";

        // Set default date range (last 30 days to next 30 days)
        FromDatePicker.Date = DateTime.Today.AddDays(-30);
        ToDatePicker.Date = DateTime.Today.AddDays(30);

        _ = LoadEnrollmentsAsync();
    }

    private async Task LoadEnrollmentsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var queryParams = new List<string>();
            
            if (fromDate.HasValue)
            {
                queryParams.Add($"from={fromDate.Value:yyyy-MM-ddTHH:mm:ssZ}");
            }
            
            if (toDate.HasValue)
            {
                queryParams.Add($"to={toDate.Value:yyyy-MM-ddTHH:mm:ssZ}");
            }

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var url = $"api/members/{_memberId}/enrollments{queryString}";

            // Add debug logging
            System.Diagnostics.Debug.WriteLine($"Loading enrollments from URL: {url}");

            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            // Debug the response
            System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"Response Content: {responseContent}");

            if (!response.IsSuccessStatusCode)
            {
                ResultLabel.TextColor = Colors.Red;
                ResultLabel.Text = $"❌ API 錯誤：{response.StatusCode} - {responseContent}";
                EnrollmentsCollection.IsVisible = false;
                NoDataLabel.IsVisible = true;
                return;
            }

            try
            {
                var enrollments = await response.Content.ReadFromJsonAsync<List<MemberEnrollmentsResponse>>();
                
                if (enrollments != null && enrollments.Count > 0)
                {
                    EnrollmentsCollection.ItemsSource = enrollments;
                    EnrollmentsCollection.IsVisible = true;
                    NoDataLabel.IsVisible = false;
                    
                    ResultLabel.TextColor = Colors.Green;
                    ResultLabel.Text = $"載入成功，共 {enrollments.Count} 筆報名紀錄";
                }
                else
                {
                    EnrollmentsCollection.ItemsSource = null;
                    EnrollmentsCollection.IsVisible = false;
                    NoDataLabel.IsVisible = true;
                    
                    ResultLabel.TextColor = Colors.Gray;
                    ResultLabel.Text = "無報名紀錄";
                    
                    // Show the raw response for debugging
                    if (!string.IsNullOrEmpty(responseContent))
                    {
                        System.Diagnostics.Debug.WriteLine($"Empty enrollments but got response: {responseContent}");
                    }
                }
            }
            catch (JsonException jsonEx)
            {
                EnrollmentsCollection.ItemsSource = null;
                EnrollmentsCollection.IsVisible = false;
                NoDataLabel.IsVisible = true;
                
                ResultLabel.TextColor = Colors.Red;
                ResultLabel.Text = $"❌ JSON 解析錯誤：{jsonEx.Message}";
                
                // Debug the JSON parsing issue
                System.Diagnostics.Debug.WriteLine($"JSON parsing failed: {jsonEx}");
                System.Diagnostics.Debug.WriteLine($"Response content: {responseContent}");
                return;
            }
        }
        catch (HttpRequestException httpEx)
        {
            EnrollmentsCollection.ItemsSource = null;
            EnrollmentsCollection.IsVisible = false;
            NoDataLabel.IsVisible = true;
            
            ResultLabel.TextColor = Colors.Red;
            ResultLabel.Text = $"❌ 網路連線錯誤：{httpEx.Message}";
            
            System.Diagnostics.Debug.WriteLine($"HTTP request failed: {httpEx}");
        }
        catch (TaskCanceledException tcEx) when (tcEx.InnerException is TimeoutException)
        {
            EnrollmentsCollection.ItemsSource = null;
            EnrollmentsCollection.IsVisible = false;
            NoDataLabel.IsVisible = true;
            
            ResultLabel.TextColor = Colors.Red;
            ResultLabel.Text = "❌ 請求逾時，請檢查網路連線或 API 服務器狀態";
            
            System.Diagnostics.Debug.WriteLine($"Request timeout: {tcEx}");
        }
        catch (TaskCanceledException tcEx)
        {
            EnrollmentsCollection.ItemsSource = null;
            EnrollmentsCollection.IsVisible = false;
            NoDataLabel.IsVisible = true;
            
            ResultLabel.TextColor = Colors.Red;
            ResultLabel.Text = "❌ 請求被取消";
            
            System.Diagnostics.Debug.WriteLine($"Request cancelled: {tcEx}");
        }
        catch (Exception ex)
        {
            EnrollmentsCollection.ItemsSource = null;
            EnrollmentsCollection.IsVisible = false;
            NoDataLabel.IsVisible = true;
            
            ResultLabel.TextColor = Colors.Red;
            ResultLabel.Text = $"❌ 載入報名紀錄失敗：{ex.Message}";
            
            // Debug the full exception
            System.Diagnostics.Debug.WriteLine($"Exception loading enrollments: {ex}");
        }
    }

    private async void OnFilterClicked(object sender, EventArgs e)
    {
        var fromDate = FromDatePicker.Date;
        var toDate = ToDatePicker.Date.AddDays(1).AddSeconds(-1); // End of selected day

        if (fromDate > toDate)
        {
            ResultLabel.TextColor = Colors.Red;
            ResultLabel.Text = "⚠️ 開始日期不能晚於結束日期";
            return;
        }

        await LoadEnrollmentsAsync(fromDate, toDate);
    }
}