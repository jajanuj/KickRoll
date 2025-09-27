using KickRoll.Api.Attributes;
using KickRoll.Api.Models;
using KickRoll.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace KickRoll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[RequireAdmin]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuditService _auditService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, IAuditService auditService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _auditService = auditService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] string? search = null, [FromQuery] string? role = null, [FromQuery] string? status = null, [FromQuery] int limit = 100)
    {
        try
        {
            var users = await _userService.GetUsersAsync(search, role, status, limit);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, new { error = "取得使用者清單失敗" });
        }
    }

    [HttpPut("{userId}/role")]
    public async Task<IActionResult> UpdateUserRole(string userId, [FromBody] UpdateUserRoleRequest request)
    {
        try
        {
            var actorUserId = HttpContext.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(actorUserId))
                return Unauthorized();

            var clientIp = GetClientIp();
            var (success, message) = await _userService.UpdateUserRoleAsync(userId, request.Role, actorUserId, clientIp);

            if (!success)
                return BadRequest(new { error = message });

            return Ok(new { message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role");
            return StatusCode(500, new { error = "角色變更失敗" });
        }
    }

    [HttpPut("{userId}/status")]
    public async Task<IActionResult> UpdateUserStatus(string userId, [FromBody] UpdateUserStatusRequest request)
    {
        try
        {
            var actorUserId = HttpContext.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(actorUserId))
                return Unauthorized();

            var clientIp = GetClientIp();
            var (success, message) = await _userService.UpdateUserStatusAsync(userId, request.Status, actorUserId, clientIp);

            if (!success)
                return BadRequest(new { error = message });

            return Ok(new { message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user status");
            return StatusCode(500, new { error = "狀態變更失敗" });
        }
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] string? actorUserId = null, [FromQuery] string? targetType = null, [FromQuery] string? targetId = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] int limit = 100)
    {
        try
        {
            var logs = await _auditService.GetAuditLogsAsync(actorUserId, targetType, targetId, from, to, limit);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs");
            return StatusCode(500, new { error = "取得審計紀錄失敗" });
        }
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