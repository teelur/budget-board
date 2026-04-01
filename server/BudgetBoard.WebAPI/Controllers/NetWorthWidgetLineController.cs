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
public class NetWorthWidgetLineController(
    ILogger<NetWorthWidgetLineController> logger,
    UserManager<ApplicationUser> userManager,
    INetWorthWidgetLineService netWorthWidgetLineService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] NetWorthWidgetLineCreateRequest request)
    {
        try
        {
            await netWorthWidgetLineService.CreateNetWorthWidgetLineAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                request
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

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] NetWorthWidgetLineUpdateRequest request)
    {
        try
        {
            await netWorthWidgetLineService.UpdateNetWorthWidgetLineAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                request
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
    public async Task<IActionResult> Delete(Guid widgetSettingsId, Guid lineId)
    {
        try
        {
            await netWorthWidgetLineService.DeleteNetWorthWidgetLineAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                widgetSettingsId,
                lineId
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

    [HttpPost]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> Reorder([FromBody] NetWorthWidgetLineReorderRequest request)
    {
        try
        {
            await netWorthWidgetLineService.ReorderNetWorthWidgetLinesAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                request
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
