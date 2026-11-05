using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using BudgetBoard.Database.Models;

namespace BudgetBoard.WebAPI.Extensions;

public static class AuthenticationConfigurationExtensions
{
    public static AuthenticationBuilder AddBudgetBoardAuthentication(
        this IServiceCollection services
    )
    {
        return services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            })
            .AddCookie(
                IdentityConstants.ApplicationScheme,
                options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                    options.SlidingExpiration = true;
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnValidatePrincipal = async context =>
                        {
                            var userManager =
                                context.HttpContext.RequestServices.GetRequiredService<
                                    UserManager<ApplicationUser>
                                >();
                            var user = await userManager.GetUserAsync(context.Principal!);
                            var securityStamp = user is null
                                ? null
                                : await userManager.GetSecurityStampAsync(user);
                            var principalSecurityStamp = context.Principal!
                                .FindFirst(
                                    new IdentityOptions().ClaimsIdentity.SecurityStampClaimType
                                )
                                ?.Value;

                            if (
                                user is null
                                || user.IsDisabled
                                || string.IsNullOrEmpty(principalSecurityStamp)
                                || !string.Equals(
                                    principalSecurityStamp,
                                    securityStamp,
                                    StringComparison.Ordinal
                                )
                            )
                            {
                                context.RejectPrincipal();
                                return;
                            }

                            context.HttpContext.Items[typeof(ApplicationUser)] = user;
                        },
                    };
                }
            )
            .AddCookie(IdentityConstants.TwoFactorRememberMeScheme)
            .AddCookie(IdentityConstants.TwoFactorUserIdScheme);
    }
}
