using System.Net.Http.Json;
using System.Text.Json;

namespace KickRoll.App.Views;

public partial class AddMemberPage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5112/") // ⚠️ 改成你的 API 埠號
    };

    public AddMemberPage()
    {
        InitializeComponent();
    }

    private async void OnSaveMemberClicked(object sender, EventArgs e)
    {
        // Reset validation labels
        NameValidationLabel.IsVisible = false;
        PhoneValidationLabel.IsVisible = false;
        ResultLabel.Text = "";

        bool isValid = true;

        // Validate name
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            NameValidationLabel.Text = "姓名不可空白";
            NameValidationLabel.IsVisible = true;
            isValid = false;
        }

        // Validate phone (must be 10 digits and start with "09")
        if (string.IsNullOrWhiteSpace(PhoneEntry.Text))
        {
            PhoneValidationLabel.Text = "電話不可空白";
            PhoneValidationLabel.IsVisible = true;
            isValid = false;
        }
        else if (!IsValidTaiwanMobile(PhoneEntry.Text.Trim()))
        {
            PhoneValidationLabel.Text = "電話號碼格式不正確，必須為10碼且前2碼為09";
            PhoneValidationLabel.IsVisible = true;
            isValid = false;
        }

        if (!isValid)
        {
            return;
        }

        // Disable button during API call
        SaveButton.IsEnabled = false;
        SaveButton.Text = "新增中...";

        try
        {
            var newMember = new
            {
                Name = NameEntry.Text.Trim(),
                Phone = PhoneEntry.Text.Trim(), // Now using Phone field for actual phone number
                Gender = "",
                Status = "active",
                TeamId = "",
                TeamIds = new List<string>(),
                Birthdate = DateTime.UtcNow
            };

            var response = await _httpClient.PostAsJsonAsync("api/members", newMember);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<dynamic>();
                await DisplayAlert("成功", "成員新增成功！", "確定");
                await Navigation.PopAsync();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ResultLabel.TextColor = Colors.Red;
                
                try
                {
                    // Try to parse JSON error response
                    var errorResponse = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(errorContent);
                    
                    if (errorResponse.TryGetProperty("error", out var errorMessage))
                    {
                        ResultLabel.Text = $"❌ {errorMessage.GetString()}";
                    }
                    else if (errorResponse.TryGetProperty("errors", out var validationErrors))
                    {
                        // Handle validation errors from API
                        var errorMessages = new List<string>();
                        foreach (var error in validationErrors.EnumerateObject())
                        {
                            if (error.Value.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var message in error.Value.EnumerateArray())
                                {
                                    errorMessages.Add(message.GetString() ?? "");
                                }
                            }
                        }
                        ResultLabel.Text = $"❌ {string.Join(", ", errorMessages)}";
                    }
                    else
                    {
                        ResultLabel.Text = $"❌ 新增失敗：{response.StatusCode}";
                    }
                }
                catch
                {
                    // If JSON parsing fails, show generic error
                    ResultLabel.Text = $"❌ 新增失敗：{response.StatusCode}";
                }
            }
        }
        catch (Exception ex)
        {
            ResultLabel.TextColor = Colors.Red;
            ResultLabel.Text = $"❌ 網路錯誤：{ex.Message}";
        }
        finally
        {
            SaveButton.IsEnabled = true;
            SaveButton.Text = "新增成員";
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private bool IsValidTaiwanMobile(string phone)
    {
        // Check if phone is exactly 10 digits and starts with "09"
        if (string.IsNullOrWhiteSpace(phone) || phone.Length != 10)
            return false;
            
        if (!phone.StartsWith("09"))
            return false;
            
        // Check if all characters are digits
        return phone.All(char.IsDigit);
    }
}