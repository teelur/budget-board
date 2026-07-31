using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

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
                }
            )
            .AddCookie(IdentityConstants.TwoFactorRememberMeScheme)
            .AddCookie(IdentityConstants.TwoFactorUserIdScheme);
    }
}
