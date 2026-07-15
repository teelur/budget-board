using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Utils;
using BudgetBoard.WebAPI.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserSettingsController(
    ILogger<UserSettingsController> logger,
    UserManager<ApplicationUser> userManager,
    IUserSettingsService userSettingsService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ApiControllerBase<UserSettingsController>(logger, logLocalizer, responseLocalizer)
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Read()
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            return Ok(await userSettingsService.ReadUserSettingsAsync(parsedUserId));
        });
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update(
        [FromBody] UserSettingsUpdateRequest userSettingsUpdateRequest
    )
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await userSettingsService.UpdateUserSettingsAsync(
                parsedUserId,
                userSettingsUpdateRequest
            );

            // If language was updated, apply it to the current request context
            if (
                userSettingsUpdateRequest.Language != null
                && userSettingsUpdateRequest.Language != "default"
            )
            {
                try
                {
                    var culture = new System.Globalization.CultureInfo(
                        userSettingsUpdateRequest.Language
                    );
                    Logger.LogInformation(
                        "{LogMessage}",
                        LogLocalizer[
                            "SettingCurrentLocaleLog",
                            $"{culture.Name} ({culture.DisplayName})"
                        ]
                    );

                    HttpContext.Features.Set<IRequestCultureFeature>(
                        new RequestCultureFeature(new RequestCulture(culture), null)
                    );
                }
                catch (Exception ex)
                {
                    Logger.LogError(
                        ex,
                        "{LogMessage}",
                        LogLocalizer[
                            "SettingCurrentLocaleErrorLog",
                            userSettingsUpdateRequest.Language
                        ]
                    );
                    return Helpers.BuildErrorResponse(ResponseLocalizer["InvalidLanguageError"]);
                }
            }

            return Ok();
        });
    }
}
