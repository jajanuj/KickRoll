using KickRoll.Api.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using Xunit;

namespace KickRoll.Api.Tests;

public class AuthorizationAttributesTests
{
    private AuthorizationFilterContext CreateAuthorizationContext(ClaimsPrincipal user)
    {
        var httpContext = new DefaultHttpContext
        {
            User = user
        };

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor()
        );

        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }

    [Fact]
    public void RequireAuth_UnauthenticatedUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var attribute = new RequireAuthAttribute();
        var user = new ClaimsPrincipal(); // Unauthenticated user
        var context = CreateAuthorizationContext(user);

        // Act
        attribute.OnAuthorization(context);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(context.Result);
    }

    [Fact]
    public void RequireAuth_NoUserIdClaim_ShouldReturnUnauthorized()
    {
        // Arrange
        var attribute = new RequireAuthAttribute();
        var identity = new ClaimsIdentity(new List<Claim>(), "test");
        var user = new ClaimsPrincipal(identity);
        var context = CreateAuthorizationContext(user);

        // Act
        attribute.OnAuthorization(context);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(context.Result);
    }

    [Fact]
    public void RequireAuth_PendingVerifyStatus_ShouldReturnForbidden()
    {
        // Arrange
        var attribute = new RequireAuthAttribute();
        var claims = new List<Claim>
        {
            new("user_id", "test-uid"),
            new("status", "PendingVerify"),
            new("email_verified", "False")
        };
        var identity = new ClaimsIdentity(claims, "test");
        var user = new ClaimsPrincipal(identity);
        var context = CreateAuthorizationContext(user);

        // Act
        attribute.OnAuthorization(context);

        // Assert
        Assert.IsType<ObjectResult>(context.Result);
        var result = context.Result as ObjectResult;
        Assert.Equal(403, result?.StatusCode);
    }

    [Fact]
    public void RequireAuth_DisabledStatus_ShouldReturnForbidden()
    {
        // Arrange
        var attribute = new RequireAuthAttribute();
        var claims = new List<Claim>
        {
            new("user_id", "test-uid"),
            new("status", "Disabled"),
            new("email_verified", "True")
        };
        var identity = new ClaimsIdentity(claims, "test");
        var user = new ClaimsPrincipal(identity);
        var context = CreateAuthorizationContext(user);

        // Act
        attribute.OnAuthorization(context);

        // Assert
        Assert.IsType<ObjectResult>(context.Result);
        var result = context.Result as ObjectResult;
        Assert.Equal(403, result?.StatusCode);
    }

    [Fact]
    public void RequireAuth_EmailNotVerified_ShouldReturnForbidden()
    {
        // Arrange
        var attribute = new RequireAuthAttribute();
        var claims = new List<Claim>
        {
            new("user_id", "test-uid"),
            new("status", "Active"),
            new("email_verified", "False")
        };
        var identity = new ClaimsIdentity(claims, "test");
        var user = new ClaimsPrincipal(identity);
        var context = CreateAuthorizationContext(user);

        // Act
        attribute.OnAuthorization(context);

        // Assert
        Assert.IsType<ObjectResult>(context.Result);
        var result = context.Result as ObjectResult;
        Assert.Equal(403, result?.StatusCode);
    }

    [Fact]
    public void RequireAuth_ValidActiveUser_ShouldAllow()
    {
        // Arrange
        var attribute = new RequireAuthAttribute();
        var claims = new List<Claim>
        {
            new("user_id", "test-uid"),
            new("status", "Active"),
            new("email_verified", "True")
        };
        var identity = new ClaimsIdentity(claims, "test");
        var user = new ClaimsPrincipal(identity);
        var context = CreateAuthorizationContext(user);

        // Act
        attribute.OnAuthorization(context);

        // Assert
        Assert.Null(context.Result);
    }

    [Fact]
    public void RequireRole_AdminRole_ShouldAllowAdmin()
    {
        // Arrange
        var attribute = new RequireRoleAttribute("Admin");
        var claims = new List<Claim>
        {
            new("user_id", "admin-uid"),
            new("status", "Active"),
            new("email_verified", "True"),
            new("role", "Admin")
        };
        var identity = new ClaimsIdentity(claims, "test");
        var user = new ClaimsPrincipal(identity);
        var context = CreateAuthorizationContext(user);

        // Act
        attribute.OnAuthorization(context);

        // Assert
        Assert.Null(context.Result);
    }

    [Fact]
    public void RequireRole_AdminRole_ShouldDenyMember()
    {
        // Arrange
        var attribute = new RequireRoleAttribute("Admin");
        var claims = new List<Claim>
        {
            new("user_id", "member-uid"),
            new("status", "Active"),
            new("email_verified", "True"),
            new("role", "Member")
        };
        var identity = new ClaimsIdentity(claims, "test");
        var user = new ClaimsPrincipal(identity);
        var context = CreateAuthorizationContext(user);

        // Act
        attribute.OnAuthorization(context);

        // Assert
        Assert.IsType<ObjectResult>(context.Result);
        var result = context.Result as ObjectResult;
        Assert.Equal(403, result?.StatusCode);
    }

    [Fact]
    public void RequireCoachOrAdmin_CoachRole_ShouldAllowCoach()
    {
        // Arrange
        var attribute = new RequireCoachOrAdminAttribute();
        var claims = new List<Claim>
        {
            new("user_id", "coach-uid"),
            new("status", "Active"),
            new("email_verified", "True"),
            new("role", "Coach")
        };
        var identity = new ClaimsIdentity(claims, "test");
        var user = new ClaimsPrincipal(identity);
        var context = CreateAuthorizationContext(user);

        // Act
        attribute.OnAuthorization(context);

        // Assert
        Assert.Null(context.Result);
    }

    [Fact]
    public void RequireAdmin_AdminRole_ShouldAllowAdmin()
    {
        // Arrange
        var attribute = new RequireAdminAttribute();
        var claims = new List<Claim>
        {
            new("user_id", "admin-uid"),
            new("status", "Active"),
            new("email_verified", "True"),
            new("role", "Admin")
        };
        var identity = new ClaimsIdentity(claims, "test");
        var user = new ClaimsPrincipal(identity);
        var context = CreateAuthorizationContext(user);

        // Act
        attribute.OnAuthorization(context);

        // Assert
        Assert.Null(context.Result);
    }

    [Fact]
    public void RequireAdmin_MemberRole_ShouldDenyMember()
    {
        // Arrange
        var attribute = new RequireAdminAttribute();
        var claims = new List<Claim>
        {
            new("user_id", "member-uid"),
            new("status", "Active"),
            new("email_verified", "True"),
            new("role", "Member")
        };
        var identity = new ClaimsIdentity(claims, "test");
        var user = new ClaimsPrincipal(identity);
        var context = CreateAuthorizationContext(user);

        // Act
        attribute.OnAuthorization(context);

        // Assert
        Assert.IsType<ObjectResult>(context.Result);
        var result = context.Result as ObjectResult;
        Assert.Equal(403, result?.StatusCode);
    }
}