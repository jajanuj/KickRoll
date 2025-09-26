using System.Net.Http.Json;
using System.Text.Json;

namespace KickRoll.App.Views;

public partial class MemberPlansPage : ContentPage
{
    #region Fields

    private readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5112/") // ⚠️ 改成你的 API 埠號
    };

    private readonly string _memberId;
    private readonly string _memberName;
    private List<MemberPlanDisplay> _allPlans = new List<MemberPlanDisplay>();

    #endregion

    #region Constructors

    public MemberPlansPage(string memberId, string memberName)
    {
        InitializeComponent();
        _memberId = memberId;
        _memberName = memberName;
        
        MemberNameLabel.Text = memberName;
        MemberIdLabel.Text = $"ID: {memberId}";
        
        StatusPicker.SelectedIndex = 0; // 預設選擇"全部"
        _ = LoadPlansAsync();
    }

    #endregion

    #region Private Methods

    private async Task LoadPlansAsync()
    {
        try
        {
            ResultLabel.Text = "載入中...";
            ResultLabel.TextColor = Colors.Blue;

            var response = await _httpClient.GetAsync($"api/members/{_memberId}/plans");
            
            if (response.IsSuccessStatusCode)
            {
                var plansJson = await response.Content.ReadAsStringAsync();
                var plans = JsonSerializer.Deserialize<List<MemberPlanResponse>>(plansJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _allPlans = plans?.Select(p => new MemberPlanDisplay
                {
                    Id = p.Id,
                    Name = p.Name,
                    Type = p.Type,
                    TotalCredits = p.TotalCredits,
                    RemainingCredits = p.RemainingCredits,
                    ValidFrom = p.ValidFrom,
                    ValidUntil = p.ValidUntil,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList() ?? new List<MemberPlanDisplay>();

                ApplyStatusFilter();
                ResultLabel.Text = $"共載入 {_allPlans.Count} 個方案";
                ResultLabel.TextColor = Colors.Green;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                ResultLabel.Text = $"載入失敗: {error}";
                ResultLabel.TextColor = Colors.Red;
            }
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"錯誤: {ex.Message}";
            ResultLabel.TextColor = Colors.Red;
        }
    }

    private void ApplyStatusFilter()
    {
        var selectedStatus = StatusPicker.SelectedItem?.ToString();
        List<MemberPlanDisplay> filteredPlans;

        if (selectedStatus == "全部" || string.IsNullOrEmpty(selectedStatus))
        {
            filteredPlans = _allPlans;
        }
        else
        {
            filteredPlans = _allPlans.Where(p => p.Status == selectedStatus).ToList();
        }

        PlansCollection.ItemsSource = filteredPlans;
    }

    private void OnStatusFilterChanged(object sender, EventArgs e)
    {
        ApplyStatusFilter();
    }

    private void OnPlanSelected(object sender, SelectionChangedEventArgs e)
    {
        // 取消選擇以避免視覺混亂
        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }
    }

    private async void OnAddPlanFabClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddMemberPlanPage(_memberId, _memberName));
    }

    private async void OnAdjustCreditsClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string planId)
        {
            var plan = _allPlans.FirstOrDefault(p => p.Id == planId);
            if (plan != null)
            {
                await ShowAdjustCreditsDialog(plan);
            }
        }
    }

    private async void OnEditPlanClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string planId)
        {
            var plan = _allPlans.FirstOrDefault(p => p.Id == planId);
            if (plan != null)
            {
                await Navigation.PushAsync(new EditMemberPlanPage(_memberId, plan));
            }
        }
    }

    private async void OnDeletePlanClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string planId)
        {
            var plan = _allPlans.FirstOrDefault(p => p.Id == planId);
            if (plan != null)
            {
                // 確認刪除
                bool confirm = await DisplayAlert("確認刪除", 
                    $"確定要刪除方案「{plan.Name}」嗎？\n此操作無法復原。", 
                    "確定", "取消");
                    
                if (confirm)
                {
                    await DeletePlan(plan);
                }
            }
        }
    }

    private async Task DeletePlan(MemberPlanDisplay plan)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/members/{_memberId}/plans/{plan.Id}");
            
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("成功", "方案刪除成功！", "確定");
                await LoadPlansAsync(); // 重新載入數據
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("錯誤", $"刪除失敗: {error}", "確定");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("錯誤", ex.Message, "確定");
        }
    }

    private async Task ShowAdjustCreditsDialog(MemberPlanDisplay plan)
    {
        try
        {
            string deltaStr = await DisplayPromptAsync(
                "調整堂數", 
                $"目前剩餘堂數: {plan.RemainingCredits}\n請輸入調整數量 (正數增加，負數減少):",
                "確定", "取消", 
                placeholder: "如: +5 或 -2", 
                keyboard: Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(deltaStr))
                return;

            // 解析數字
            if (!int.TryParse(deltaStr.Replace("+", ""), out int delta))
            {
                await DisplayAlert("錯誤", "請輸入有效的數字", "確定");
                return;
            }

            var reason = await DisplayPromptAsync(
                "調整原因", 
                "請輸入調整原因 (可選):",
                "確定", "取消", 
                placeholder: "如: 上課出席、手動補償等");

            var adjustRequest = new
            {
                delta = delta,
                reason = reason ?? "手動調整"
            };

            var response = await _httpClient.PostAsJsonAsync($"api/members/{_memberId}/plans/{plan.Id}:adjust", adjustRequest);
            
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("成功", $"堂數調整成功！調整: {delta:+0;-0;0}", "確定");
                await LoadPlansAsync(); // 重新載入數據
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("錯誤", $"調整失敗: {error}", "確定");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("錯誤", ex.Message, "確定");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPlansAsync(); // 每次顯示頁面時重新載入
    }

    #endregion
}

#region Data Models

public class MemberPlanResponse
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public string Name { get; set; } = "";
    public int? TotalCredits { get; set; }
    public int RemainingCredits { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class MemberPlanDisplay
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public string Name { get; set; } = "";
    public int? TotalCredits { get; set; }
    public int RemainingCredits { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Display properties for UI binding
    public string TypeDisplay => Type == "credit_pack" ? "計次" : "包月";
    public Color TypeColor => Type == "credit_pack" ? Colors.Blue : Colors.Purple;
    
    public string StatusDisplay => Status switch
    {
        "active" => "啟用",
        "expired" => "過期",
        "suspended" => "暫停",
        _ => Status
    };
    
    public Color StatusColor => Status switch
    {
        "active" => Colors.Green,
        "expired" => Colors.Red,
        "suspended" => Colors.Orange,
        _ => Colors.Gray
    };

    public bool ShowCredits => Type == "credit_pack";
    public string CreditsInfo => TotalCredits.HasValue 
        ? $"剩餘: {RemainingCredits} / {TotalCredits} 堂" 
        : $"剩餘: {RemainingCredits} 堂";

    public bool ShowValidUntil => ValidUntil.HasValue;
    public string ValidUntilDisplay => ValidUntil?.ToString("yyyy/MM/dd") ?? "";
}

#endregion