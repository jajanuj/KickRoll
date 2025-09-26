using System.Net.Http.Json;
using System.Text.Json;

namespace KickRoll.App.Views;

public partial class MemberEnrollmentsPage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5112/") // ⚠️ 改成你的 API 埠號
    };

    private readonly string _memberId;
    private readonly string _memberName;

    public class SessionEnrollmentDisplay
    {
        public string Id { get; set; } = default!;
        public string CourseId { get; set; } = default!;
        public string TeamId { get; set; } = default!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Capacity { get; set; }
        public int EnrolledCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class MemberEnrollmentDisplay
    {
        public string EnrollmentId { get; set; } = default!;
        public string SessionId { get; set; } = default!;
        public string Status { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public SessionEnrollmentDisplay Session { get; set; } = default!;

        public Color StatusColor => Status switch
        {
            "enrolled" => Colors.Green,
            "cancelled" => Colors.Orange,
            _ => Colors.Gray
        };
    }

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

            var enrollments = await _httpClient.GetFromJsonAsync<List<MemberEnrollmentDisplay>>(url);
            
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
            }
        }
        catch (Exception ex)
        {
            EnrollmentsCollection.ItemsSource = null;
            EnrollmentsCollection.IsVisible = false;
            NoDataLabel.IsVisible = true;
            
            ResultLabel.TextColor = Colors.Red;
            ResultLabel.Text = $"❌ 載入報名紀錄失敗：{ex.Message}";
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