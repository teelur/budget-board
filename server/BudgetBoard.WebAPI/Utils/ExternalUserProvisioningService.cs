using System.Security.Claims;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.WebAPI.Resources;
using BudgetBoard.WebAPI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.Utils
{
    public class ExternalUserProvisioningService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IWidgetSettingsService widgetSettingsService,
        ISuperuserService superuserService,
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
        private readonly ISuperuserService _superuserService =
            superuserService ?? throw new ArgumentNullException(nameof(superuserService));
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

            // Find or create local user. A missing user is re-checked inside the
            // transaction-scoped bootstrap lock to coordinate with local registration.
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Check if new user creation is disabled
                var disableNewUsers = _configuration.GetValue<bool>("DISABLE_NEW_USERS");
                if (disableNewUsers)
                {
                    _logger.LogWarning("{LogMessage}", _logLocalizer["NewUserCreationDisabledLog"]);
                    return ExternalUserProvisioningResult.Failed();
                }
                var provisioningState =
                    await _superuserService.ExecuteFirstUserBootstrapAsync(
                        () =>
                            CreateOrCompleteUserAsync(
                                email,
                                providerKey,
                                schemeName
                            ),
                        httpContext.RequestAborted
                    );
                if (!provisioningState.Result.Succeeded)
                {
                    return provisioningState.Result;
                }

                user = provisioningState.User!;
            }
            else
            {
                var provisioningState = await CompleteUserProvisioningAsync(
                    user,
                    false,
                    providerKey,
                    schemeName
                );
                if (!provisioningState.Result.Succeeded)
                {
                    return provisioningState.Result;
                }

                user = provisioningState.User!;
            }

            // Sign into application cookie
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName ?? email),
                new(ClaimTypes.Email, user.Email ?? email),
            };
            var securityStamp = await _userManager.GetSecurityStampAsync(user);
            if (!string.IsNullOrEmpty(securityStamp))
            {
                claims.Add(
                    new Claim(
                        new IdentityOptions().ClaimsIdentity.SecurityStampClaimType,
                        securityStamp
                    )
                );
            }
            claims.AddRange(
                (await _userManager.GetRolesAsync(user)).Select(role => new Claim(
                    ClaimTypes.Role,
                    role
                ))
            );

            var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            await httpContext.SignInAsync(
                IdentityConstants.ApplicationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = isPersistent }
            );

            return ExternalUserProvisioningResult.Success();
        }

        private async Task<ProvisioningState> CreateOrCompleteUserAsync(
            string email,
            string providerKey,
            string schemeName
        )
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
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
                    return ProvisioningState.Failed();
                }

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

                await _superuserService.EnsureFirstUserSuperuserAsync(user);
                _logger.LogInformation("{LogMessage}", _logLocalizer["UserProvisionedLog"]);
                return await CompleteUserProvisioningAsync(
                    user,
                    true,
                    providerKey,
                    schemeName
                );
            }

            return await CompleteUserProvisioningAsync(user, false, providerKey, schemeName);
        }

        private async Task<ProvisioningState> CompleteUserProvisioningAsync(
            ApplicationUser user,
            bool wasUserCreated,
            string providerKey,
            string schemeName
        )
        {
            if (user.IsDisabled)
            {
                _logger.LogWarning("OIDC login rejected for disabled user {UserId}.", user.Id);
                return ProvisioningState.Failed();
            }

            if (!wasUserCreated)
            {
                _logger.LogInformation("{LogMessage}", _logLocalizer["UserFoundLog"]);
            }

            var userLogins = await _userManager.GetLoginsAsync(user);
            var hasLogin = userLogins.Any(l =>
                l.LoginProvider == schemeName && l.ProviderKey == providerKey
            );
            if (hasLogin)
            {
                return ProvisioningState.Succeeded(user);
            }

            // Existing local accounts must link OIDC explicitly through the authenticated settings flow.
            if (!wasUserCreated)
            {
                _logger.LogInformation(
                    "{LogMessage}",
                    _logLocalizer["OidcExplicitLinkRequiredLog", user.Id]
                );
                return ProvisioningState.ExplicitLinkingRequired();
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
                return ProvisioningState.Failed();
            }

            return ProvisioningState.Succeeded(user);
        }

        private sealed record ProvisioningState(
            ApplicationUser? User,
            ExternalUserProvisioningResult Result
        )
        {
            public static ProvisioningState Succeeded(ApplicationUser user) =>
                new(user, ExternalUserProvisioningResult.Success());

            public static ProvisioningState Failed() =>
                new(null, ExternalUserProvisioningResult.Failed());

            public static ProvisioningState ExplicitLinkingRequired() =>
                new(null, ExternalUserProvisioningResult.ExplicitLinkingRequired());
        }
    }
}
