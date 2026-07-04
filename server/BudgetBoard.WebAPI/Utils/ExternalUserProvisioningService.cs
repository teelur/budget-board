using System.Security.Claims;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.WebAPI.Resources;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.Utils
{
    public class ExternalUserProvisioningService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IWidgetSettingsService widgetSettingsService,
        ILogger<ExternalUserProvisioningService> logger,
        IStringLocalizer<ApiLogStrings> logLocalizer
    ) : IExternalUserProvisioningService
    {
        private readonly UserManager<ApplicationUser> _userManager =
            userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly IConfiguration _configuration =
            configuration ?? throw new ArgumentNullException(nameof(configuration));
        private readonly IWidgetSettingsService _widgetSettingsService =
            widgetSettingsService ?? throw new ArgumentNullException(nameof(widgetSettingsService));
        private readonly ILogger<ExternalUserProvisioningService> _logger =
            logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IStringLocalizer<ApiLogStrings> _logLocalizer = logLocalizer;

        public async Task<ExternalUserProvisioningResult> ProvisionExternalUserAsync(
            ClaimsPrincipal principal,
            HttpContext httpContext,
            string schemeName,
            bool isPersistent = false
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
                return ExternalUserProvisioningResult.Failed();
            }

            // Find or create local user
            var user = await _userManager.FindByEmailAsync(email);
            var wasUserCreated = false;
            if (user == null)
            {
                // Check if new user creation is disabled
                var disableNewUsers = _configuration.GetValue<bool>("DISABLE_NEW_USERS");
                if (disableNewUsers)
                {
                    _logger.LogWarning("{LogMessage}", _logLocalizer["NewUserCreationDisabledLog"]);
                    return ExternalUserProvisioningResult.Failed();
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
                    return ExternalUserProvisioningResult.Failed();
                }

                wasUserCreated = true;

                try
                {
                    foreach (var layout in WidgetSettingsHelpers.DefaultLayouts)
                    {
                        await _widgetSettingsService.CreateWidgetSettingsAsync(
                            user.Id,
                            new WidgetSettingsCreateRequest
                            {
                                WidgetType = layout.WidgetType,
                                LgX = layout.LgX,
                                LgY = layout.LgY,
                                LgW = layout.LgW,
                                LgH = layout.LgH,
                                SmY = layout.SmY,
                                SmH = layout.SmH,
                            }
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        "{LogMessage}",
                        _logLocalizer["DefaultWidgetSeedingFailedLog", user.Id, ex.Message]
                    );
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
                // Existing local accounts must link OIDC explicitly through the authenticated settings flow.
                if (!wasUserCreated)
                {
                    _logger.LogInformation(
                        "{LogMessage}",
                        _logLocalizer["OidcExplicitLinkRequiredLog", user.Id]
                    );
                    return ExternalUserProvisioningResult.ExplicitLinkingRequired();
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
                    return ExternalUserProvisioningResult.Failed();
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
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = isPersistent }
            );

            return ExternalUserProvisioningResult.Success();
        }
    }
}
