using System.Security.Claims;
using BudgetBoard.Database.Models;
using BudgetBoard.WebAPI.Resources;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.Utils
{
    public class ExternalUserProvisioningService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger<ExternalUserProvisioningService> logger,
        IStringLocalizer<ApiLogStrings> logLocalizer
    ) : IExternalUserProvisioningService
    {
        private readonly UserManager<ApplicationUser> _userManager =
            userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly IConfiguration _configuration =
            configuration ?? throw new ArgumentNullException(nameof(configuration));
        private readonly ILogger<ExternalUserProvisioningService> _logger =
            logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IStringLocalizer<ApiLogStrings> _logLocalizer = logLocalizer;

        public async Task<bool> ProvisionExternalUserAsync(
            ClaimsPrincipal principal,
            HttpContext httpContext,
            string schemeName
        )
        {
            ArgumentNullException.ThrowIfNull(principal);
            ArgumentNullException.ThrowIfNull(httpContext);
            ArgumentNullException.ThrowIfNull(schemeName);

            // Stable identifier from provider and email
            var providerKey =
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst("sub")?.Value;
            var email =
                principal.FindFirst(ClaimTypes.Email)?.Value ?? principal.FindFirst("email")?.Value;

            if (string.IsNullOrEmpty(providerKey) || string.IsNullOrEmpty(email))
            {
                _logger.LogWarning(
                    "{LogMessage}",
                    _logLocalizer["ExternalProviderClaimsMissingLog"]
                );
                return false;
            }

            // Find or create local user
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Check if new user creation is disabled
                var disableNewUsers = _configuration.GetValue<bool>("DISABLE_NEW_USERS");
                if (disableNewUsers)
                {
                    _logger.LogWarning("{LogMessage}", _logLocalizer["NewUserCreationDisabledLog"]);
                    return false;
                }

                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    _logger.LogWarning(
                        "{LogMessage}",
                        _logLocalizer[
                            "UserCreationErrorLog",
                            string.Join(", ", createResult.Errors.Select(e => e.Description))
                        ]
                    );
                    return false;
                }

                _logger.LogInformation("{LogMessage}", _logLocalizer["UserProvisionedLog"]);
            }
            else
            {
                _logger.LogInformation("{LogMessage}", _logLocalizer["UserFoundLog"]);
            }

            // Ensure external login association exists
            var userLogins = await _userManager.GetLoginsAsync(user);
            var hasLogin = userLogins.Any(l =>
                l.LoginProvider == schemeName && l.ProviderKey == providerKey
            );
            if (!hasLogin)
            {
                // Check if adding new logins is disabled
                var disableNewUsers = _configuration.GetValue<bool>("DISABLE_NEW_USERS");
                if (disableNewUsers)
                {
                    _logger.LogWarning("{LogMessage}", _logLocalizer["AddingLoginDisabledLog"]);
                    return false;
                }

                var loginInfo = new UserLoginInfo(schemeName, providerKey, schemeName);
                var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
                if (!addLoginResult.Succeeded)
                {
                    _logger.LogWarning(
                        "{LogMessage}",
                        _logLocalizer[
                            "AddingLoginErrorLog",
                            user.Id,
                            string.Join(", ", addLoginResult.Errors.Select(e => e.Description))
                        ]
                    );
                    return false;
                }
            }

            // Sign into application cookie
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName ?? email),
                new(ClaimTypes.Email, user.Email ?? email),
            };

            var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            await httpContext.SignInAsync(
                IdentityConstants.ApplicationScheme,
                new ClaimsPrincipal(identity)
            );

            return true;
        }
    }
}
