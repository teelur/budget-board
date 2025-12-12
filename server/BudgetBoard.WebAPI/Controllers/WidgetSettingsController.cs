using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Utils;
using BudgetBoard.WebAPI.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WidgetSettingsController(
    ILogger<WidgetSettingsController> logger,
    UserManager<ApplicationUser> userManager,
    IWidgetSettingsService widgetSettingsService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] WidgetSettingsCreateRequest<NetWorthWidgetConfiguration> newWidget
    )
    {
        try
        {
            await widgetSettingsService.CreateWidgetSettingsAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                newWidget
            );
            return Ok();
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{LogMessage}", logLocalizer["UnexpectedErrorLog"]);
            return Helpers.BuildErrorResponse(responseLocalizer["UnexpectedServerError"]);
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Read()
    {
        try
        {
            return Ok(
                await widgetSettingsService.ReadWidgetSettingsAsync(
                    new Guid(userManager.GetUserId(User) ?? string.Empty)
                )
            );
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{LogMessage}", logLocalizer["UnexpectedErrorLog"]);
            return Helpers.BuildErrorResponse(responseLocalizer["UnexpectedServerError"]);
        }
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update(
        [FromBody] WidgetSettingsUpdateRequest<NetWorthWidgetConfiguration> editedWidget
    )
    {
        try
        {
            await widgetSettingsService.UpdateWidgetSettingsAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                editedWidget
            );
            return Ok();
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{LogMessage}", logLocalizer["UnexpectedErrorLog"]);
            return Helpers.BuildErrorResponse(responseLocalizer["UnexpectedServerError"]);
        }
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete(Guid widgetGuid)
    {
        try
        {
            await widgetSettingsService.DeleteWidgetSettingsAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                widgetGuid
            );
            return Ok();
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{LogMessage}", logLocalizer["UnexpectedErrorLog"]);
            return Helpers.BuildErrorResponse(responseLocalizer["UnexpectedServerError"]);
        }
    }
}
