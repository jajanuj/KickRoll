using System.Net.Http.Json;
using System.Text.RegularExpressions;
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
        EmailValidationLabel.IsVisible = false;
        ResultLabel.Text = "";

        bool isValid = true;

        // Validate name
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            NameValidationLabel.Text = "姓名不可空白";
            NameValidationLabel.IsVisible = true;
            isValid = false;
        }

        // Validate email format
        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            EmailValidationLabel.Text = "Email不可空白";
            EmailValidationLabel.IsVisible = true;
            isValid = false;
        }
        else if (!IsValidEmail(EmailEntry.Text))
        {
            EmailValidationLabel.Text = "Email格式不正確";
            EmailValidationLabel.IsVisible = true;
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
                Phone = EmailEntry.Text.Trim(), // Using Phone field to store Email as per existing structure
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

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Use regex for basic email validation
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}