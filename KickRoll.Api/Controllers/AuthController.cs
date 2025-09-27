using KickRoll.Api.Attributes;
using KickRoll.Api.Models;
using KickRoll.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace KickRoll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IUserService userService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
    {
        try
        {
            var clientIp = GetClientIp();
            var (success, message, user) = await _authService.RegisterUserAsync(request, clientIp);

            if (!success)
                return BadRequest(new { error = message });

            return Ok(new { message, user });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new { error = "註冊過程中發生錯誤" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
    {
        try
        {
            var clientIp = GetClientIp();
            var (success, message, user, token) = await _authService.LoginUserAsync(request, clientIp);

            if (!success)
                return BadRequest(new { error = message });

            return Ok(new { message, user, token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { error = "登入過程中發生錯誤" });
        }
    }

    [HttpPost("verify-email/{userId}")]
    public async Task<IActionResult> VerifyEmail(string userId)
    {
        try
        {
            var (success, message) = await _authService.VerifyEmailAsync(userId);

            if (!success)
                return BadRequest(new { error = message });

            return Ok(new { message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification");
            return StatusCode(500, new { error = "電子郵件驗證過程中發生錯誤" });
        }
    }

    [HttpPost("send-verification/{userId}")]
    public async Task<IActionResult> SendEmailVerification(string userId)
    {
        try
        {
            var (success, message) = await _authService.SendEmailVerificationAsync(userId);

            if (!success)
                return BadRequest(new { error = message });

            return Ok(new { message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending verification email");
            return StatusCode(500, new { error = "驗證信寄送過程中發生錯誤" });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] PasswordResetRequest request)
    {
        try
        {
            var (success, message) = await _authService.SendPasswordResetAsync(request.Email);

            if (!success)
                return BadRequest(new { error = message });

            return Ok(new { message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");
            return StatusCode(500, new { error = "密碼重設過程中發生錯誤" });
        }
    }

    [HttpGet("profile")]
    [RequireAuth]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = HttpContext.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "使用者 ID 不存在" });

            var (success, message, user) = await _authService.GetUserByIdAsync(userId);

            if (!success)
                return NotFound(new { error = message });

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile");
            return StatusCode(500, new { error = "取得使用者資料過程中發生錯誤" });
        }
    }

    [HttpPost("setup-admin")]
    public async Task<IActionResult> SetupInitialAdmin([FromBody] UserRegistrationRequest request)
    {
        try
        {
            // This endpoint should be protected by environment variable or config
            var allowSetup = Environment.GetEnvironmentVariable("ALLOW_ADMIN_SETUP") == "true";
            if (!allowSetup)
            {
                return Forbid("初始管理員設定已被停用");
            }

            var (success, message) = await _userService.CreateInitialAdminAsync(request.Email, request.Password, request.Name);

            if (!success)
                return BadRequest(new { error = message });

            return Ok(new { message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up initial admin");
            return StatusCode(500, new { error = "管理員設定過程中發生錯誤" });
        }
    }

    [HttpPost("logout")]
    [RequireAuth]
    public IActionResult Logout()
    {
        // Firebase Auth handles token invalidation on client side
        // We just return success here
        return Ok(new { message = "登出成功" });
    }

    private string? GetClientIp()
    {
        var forwardedHeader = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedHeader))
        {
            return forwardedHeader.Split(',')[0].Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}