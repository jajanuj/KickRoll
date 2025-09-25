using System.Net.Http.Json;

namespace KickRoll.App.Views;

public partial class EditMemberPlanPage : ContentPage
{
    #region Fields

    private readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5112/") // ⚠️ 改成你的 API 埠號
    };

    private readonly string _memberId;
    private readonly MemberPlanDisplay _plan;

    #endregion

    #region Constructors

    public EditMemberPlanPage(string memberId, MemberPlanDisplay plan)
    {
        InitializeComponent();
        _memberId = memberId;
        _plan = plan;
        
        // 設定顯示資訊
        PlanNameLabel.Text = plan.Name;
        PlanIdLabel.Text = $"ID: {plan.Id}";
        PlanTypeLabel.Text = $"類型: {plan.TypeDisplay}";
        
        // 設定編輯欄位
        RemainingCreditsEntry.Text = plan.RemainingCredits.ToString();
        StatusPicker.SelectedItem = plan.Status;
    }

    #endregion

    #region Event Handlers

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // 重設驗證標籤
        RemainingCreditsValidationLabel.IsVisible = false;
        ResultLabel.Text = "";

        bool isValid = true;

        // 驗證剩餘堂數
        if (!int.TryParse(RemainingCreditsEntry.Text, out int remainingCredits) || remainingCredits < 0)
        {
            RemainingCreditsValidationLabel.Text = "剩餘堂數必須為非負整數";
            RemainingCreditsValidationLabel.IsVisible = true;
            isValid = false;
        }

        if (!isValid)
        {
            return;
        }

        // 停用按鈕防止重複提交
        SaveButton.IsEnabled = false;
        SaveButton.Text = "儲存中...";

        try
        {
            var updateRequest = new
            {
                remainingCredits = remainingCredits,
                status = StatusPicker.SelectedItem?.ToString()
            };

            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"api/members/{_memberId}/plans/{_plan.Id}")
            {
                Content = JsonContent.Create(updateRequest)
            });

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("成功", "方案更新成功！", "確定");
                await Navigation.PopAsync();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ResultLabel.Text = $"更新失敗: {errorContent}";
                ResultLabel.TextColor = Colors.Red;
            }
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"錯誤: {ex.Message}";
            ResultLabel.TextColor = Colors.Red;
        }
        finally
        {
            SaveButton.IsEnabled = true;
            SaveButton.Text = "儲存變更";
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    #endregion
}