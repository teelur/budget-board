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
    ILogger<AccountController> logger,
    UserManager<ApplicationUser> userManager,
    IUserSettingsService userSettingsService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ControllerBase
{
    private readonly ILogger<AccountController> _logger = logger;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IUserSettingsService _userSettingsService = userSettingsService;
    private readonly IStringLocalizer<ApiLogStrings> _logLocalizer = logLocalizer;
    private readonly IStringLocalizer<ApiResponseStrings> _responseLocalizer = responseLocalizer;

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Read()
    {
        try
        {
            return Ok(
                await _userSettingsService.ReadUserSettingsAsync(
                    new Guid(_userManager.GetUserId(User) ?? string.Empty)
                )
            );
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{LogMessage}", _logLocalizer["UnexpectedErrorLog"]);
            return Helpers.BuildErrorResponse(_responseLocalizer["UnexpectedServerError"]);
        }
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update(
        [FromBody] UserSettingsUpdateRequest userSettingsUpdateRequest
    )
    {
        try
        {
            await _userSettingsService.UpdateUserSettingsAsync(
                new Guid(_userManager.GetUserId(User) ?? string.Empty),
                userSettingsUpdateRequest
            );

            // If language was updated, apply it to the current request context
            if (
                userSettingsUpdateRequest.Language != null
                && userSettingsUpdateRequest.Language != "default"
            )
            {
                var culture = new System.Globalization.CultureInfo(
                    userSettingsUpdateRequest.Language
                );
                _logger.LogInformation(
                    "{LogMessage}",
                    _logLocalizer[
                        "SettingCurrentLocaleLog",
                        $"{culture.Name} ({culture.DisplayName})"
                    ]
                );

                HttpContext.Features.Set<IRequestCultureFeature>(
                    new RequestCultureFeature(new RequestCulture(culture), null)
                );
            }

            return Ok();
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{LogMessage}", _logLocalizer["UnexpectedErrorLog"]);
            return Helpers.BuildErrorResponse(_responseLocalizer["UnexpectedServerError"]);
        }
    }
}
