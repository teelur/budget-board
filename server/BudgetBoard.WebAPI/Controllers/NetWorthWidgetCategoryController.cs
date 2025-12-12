using BudgetBoard.Database.Models;
using BudgetBoard.Service;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Utils;
using BudgetBoard.WebAPI.Controllers;
using BudgetBoard.WebAPI.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NetWorthWidgetCategoryController(
    ILogger<NetWorthWidgetCategoryController> logger,
    UserManager<ApplicationUser> userManager,
    INetWorthWidgetCategoryService netWorthWidgetCategoryService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] NetWorthWidgetCategoryCreateRequest request)
    {
        try
        {
            await netWorthWidgetCategoryService.CreateNetWorthWidgetCategoryAsync(
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
    public async Task<IActionResult> Update([FromBody] NetWorthWidgetCategoryUpdateRequest request)
    {
        try
        {
            await netWorthWidgetCategoryService.UpdateNetWorthWidgetCategoryAsync(
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
    public async Task<IActionResult> Delete(Guid widgetSettingsId, Guid lineId, Guid categoryId)
    {
        try
        {
            await netWorthWidgetCategoryService.DeleteNetWorthWidgetCategoryAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                widgetSettingsId,
                lineId,
                categoryId
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
