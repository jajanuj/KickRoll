using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace KickRoll.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireAuthAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Check if user is authenticated
        if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedObjectResult(new { error = "未授權存取" });
            return;
        }

        // Check if user ID exists in claims
        var userId = context.HttpContext.User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new UnauthorizedObjectResult(new { error = "使用者 ID 不存在" });
            return;
        }

        // Check status claim
        var status = context.HttpContext.User.FindFirst("status")?.Value;
        if (status != "Active")
        {
            if (status == "PendingVerify")
            {
                context.Result = new ObjectResult(new { error = "請先完成電子郵件驗證", requiresVerification = true }) { StatusCode = 403 };
            }
            else if (status == "Disabled")
            {
                context.Result = new ObjectResult(new { error = "此帳號已被停用，請聯絡管理員" }) { StatusCode = 403 };
            }
            else
            {
                context.Result = new UnauthorizedObjectResult(new { error = "帳號狀態異常" });
            }
            return;
        }

        // Check email verification
        var emailVerified = context.HttpContext.User.FindFirst("email_verified")?.Value;
        if (emailVerified != "True")
        {
            context.Result = new ObjectResult(new { error = "請先完成電子郵件驗證", requiresVerification = true }) { StatusCode = 403 };
        }
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireRoleAttribute : RequireAuthAttribute
{
    private readonly string[] _requiredRoles;

    public RequireRoleAttribute(params string[] roles)
    {
        _requiredRoles = roles;
    }

    public new void OnAuthorization(AuthorizationFilterContext context)
    {
        // First check basic auth requirements
        base.OnAuthorization(context);
        if (context.Result != null) return; // Failed auth check

        // Check role
        var userRole = context.HttpContext.User.FindFirst("role")?.Value;
        if (string.IsNullOrEmpty(userRole) || !_requiredRoles.Contains(userRole))
        {
            context.Result = new ObjectResult(new { error = "權限不足" }) { StatusCode = 403 };
        }
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireAdminAttribute : RequireRoleAttribute
{
    public RequireAdminAttribute() : base("Admin") { }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireCoachOrAdminAttribute : RequireRoleAttribute
{
    public RequireCoachOrAdminAttribute() : base("Coach", "Admin") { }
}