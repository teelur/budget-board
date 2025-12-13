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
public class NetWorthWidgetGroupController(
    ILogger<NetWorthWidgetGroupController> logger,
    UserManager<ApplicationUser> userManager,
    INetWorthWidgetGroupService netWorthWidgetGroupService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ControllerBase
{
    [HttpPost]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> Reorder([FromBody] NetWorthWidgetGroupReorderRequest request)
    {
        try
        {
            await netWorthWidgetGroupService.ReorderNetWorthWidgetGroupsAsync(
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
