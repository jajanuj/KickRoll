using System.Net.Http.Json;

namespace KickRoll.App.Views;

public partial class AddMemberPlanPage : ContentPage
{
    #region Fields

    private readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5112/") // ⚠️ 改成你的 API 埠號
    };

    private readonly string _memberId;
    private readonly string _memberName;

    #endregion

    #region Constructors

    public AddMemberPlanPage(string memberId, string memberName)
    {
        InitializeComponent();
        _memberId = memberId;
        _memberName = memberName;
        
        MemberNameLabel.Text = memberName;
        MemberIdLabel.Text = $"ID: {memberId}";
        
        // 預設值
        StatusPicker.SelectedItem = "active";
        TypePicker.SelectedIndex = 0; // 預設選擇 credit_pack
        ValidFromPicker.Date = DateTime.Today;
        ValidUntilPicker.Date = DateTime.Today.AddMonths(6);
    }

    #endregion

    #region Event Handlers

    private void OnTypeChanged(object sender, EventArgs e)
    {
        var selectedType = TypePicker.SelectedItem?.ToString();
        bool isCreditPack = selectedType == "計次";
        
        // 顯示/隱藏總堂數欄位
        TotalCreditsSection.IsVisible = isCreditPack;
        
        // 更新提示文字
        if (isCreditPack)
        {
            RemainingCreditsHint.Text = "計次：設定初始剩餘堂數（通常等於總堂數）";
            ValidUntilLabel.Text = "設定到期日期 (計次可選)";
            ValidUntilCheckBox.IsChecked = false;
        }
        else
        {
            RemainingCreditsHint.Text = "包月：通常設為 0（不按次計費）";
            ValidUntilLabel.Text = "設定到期日期 (包月必須設定) *";
            ValidUntilCheckBox.IsChecked = true;
        }
    }

    private void OnValidFromCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        ValidFromPicker.IsVisible = e.Value;
    }

    private void OnValidUntilCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        ValidUntilPicker.IsVisible = e.Value;
    }

    private async void OnSavePlanClicked(object sender, EventArgs e)
    {
        // 重設驗證標籤
        NameValidationLabel.IsVisible = false;
        RemainingCreditsValidationLabel.IsVisible = false;
        ValidUntilValidationLabel.IsVisible = false;
        ResultLabel.Text = "";

        bool isValid = true;

        // 驗證方案名稱
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            NameValidationLabel.Text = "方案名稱不可空白";
            NameValidationLabel.IsVisible = true;
            isValid = false;
        }

        // 驗證剩餘堂數
        if (!int.TryParse(RemainingCreditsEntry.Text, out int remainingCredits) || remainingCredits < 0)
        {
            RemainingCreditsValidationLabel.Text = "剩餘堂數必須為非負整數";
            RemainingCreditsValidationLabel.IsVisible = true;
            isValid = false;
        }

        // 驗證方案類型
        var selectedType = TypePicker.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(selectedType))
        {
            ResultLabel.Text = "請選擇方案類型";
            isValid = false;
        }

        // 期限票必須設定到期日期
        if (selectedType == "包月" && !ValidUntilCheckBox.IsChecked)
        {
            ValidUntilValidationLabel.Text = "包月必須設定到期日期";
            ValidUntilValidationLabel.IsVisible = true;
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
            // 準備請求資料
            var newPlan = new
            {
                type = GetApiTypeFromDisplayName(selectedType),
                name = NameEntry.Text.Trim(),
                totalCredits = TotalCreditsSection.IsVisible && int.TryParse(TotalCreditsEntry.Text, out int total) ? total : (int?)null,
                remainingCredits = remainingCredits,
                validFrom = ValidFromCheckBox.IsChecked ? DateTime.SpecifyKind(ValidFromPicker.Date, DateTimeKind.Utc) : (DateTime?)null,
                validUntil = ValidUntilCheckBox.IsChecked ? DateTime.SpecifyKind(ValidUntilPicker.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc) : (DateTime?)null,
                status = StatusPicker.SelectedItem?.ToString() ?? "active"
            };

            var response = await _httpClient.PostAsJsonAsync($"api/members/{_memberId}/plans", newPlan);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("成功", "方案新增成功！", "確定");
                await Navigation.PopAsync();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ResultLabel.Text = $"新增失敗: {errorContent}";
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
            SaveButton.Text = "儲存方案";
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private string GetApiTypeFromDisplayName(string displayName)
    {
        return displayName switch
        {
            "計次" => "credit_pack",
            "包月" => "time_pass",
            _ => "credit_pack" // default fallback
        };
    }

    #endregion
}