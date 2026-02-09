using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

namespace BudgetBoard.WebAPI.Utils;

/// <summary>
/// Custom culture provider that reads the user's language preference from their UserSettings.
/// </summary>
public class UserLanguageCultureProvider : RequestCultureProvider
{
    public override async Task<ProviderCultureResult?> DetermineProviderCultureResult(
        HttpContext httpContext
    )
    {
        if (httpContext.User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userManager = httpContext.RequestServices.GetService<UserManager<ApplicationUser>>();
        if (userManager == null)
        {
            return null;
        }

        var userId = userManager.GetUserId(httpContext.User);
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        var dbContext = httpContext.RequestServices.GetService<UserDataContext>();
        if (dbContext == null)
        {
            return null;
        }

        try
        {
            var userSettings = await dbContext
                .UserSettings.AsNoTracking()
                .FirstOrDefaultAsync(us => us.UserID == new Guid(userId));

            if (userSettings?.Language != null && userSettings.Language != "default")
            {
                return new ProviderCultureResult(userSettings.Language);
            }
        }
        catch (Exception ex)
        {
            var logger = httpContext.RequestServices.GetService<
                ILogger<UserLanguageCultureProvider>
            >();
            logger?.LogError(
                ex,
                "Error determining culture from user settings for user {UserId}",
                userId
            );

            // If there's any error reading the database, fall through to the next provider
        }

        // User has no language preference or set to "default", fall back to browser locale
        return null;
    }
}
