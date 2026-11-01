using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using BudgetBoard.Database.Models;
using BudgetBoard.WebAPI.Resources;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Middleware;

public sealed class MustChangePasswordMiddleware(
    RequestDelegate next,
    IStringLocalizer<ApiResponseStrings> localizer
)
{
    public async Task InvokeAsync(
        HttpContext context,
        UserManager<ApplicationUser> userManager
    )
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user is null || user.IsDisabled)
            {
                await context.SignOutAsync(IdentityConstants.ApplicationScheme);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            if (user.MustChangePassword && !IsPasswordChangeAllowed(context))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(
                    new
                    {
                        code = "PasswordChangeRequired",
                        message = localizer["PasswordChangeRequired"].Value,
                    }
                );
                return;
            }
        }

        await next(context);
    }

    private static bool IsPasswordChangeAllowed(HttpContext context)
    {
        var path = context.Request.Path;
        return (
                path.Equals("/api/manage/info", StringComparison.OrdinalIgnoreCase)
                && HttpMethods.IsPost(context.Request.Method)
            )
            || path.Equals("/api/logout", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/api/isAuthenticated", StringComparison.OrdinalIgnoreCase)
            || (
                path.Equals("/api/applicationUser", StringComparison.OrdinalIgnoreCase)
                && HttpMethods.IsGet(context.Request.Method)
            );
    }
}
