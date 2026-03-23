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

[Route("api/[controller]/[action]")]
[ApiController]
public class SyncController(
    ILogger<SyncController> logger,
    UserManager<ApplicationUser> userManager,
    ISyncService syncService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Run()
    {
        try
        {
            return Ok(
                await syncService.SyncAsync(
                    new Guid(userManager.GetUserId(User) ?? string.Empty),
                    force: true
                )
            );
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message, bbex.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{LogMessage}", logLocalizer["UnexpectedErrorLog"]);
            return Helpers.BuildErrorResponse(responseLocalizer["UnexpectedServerError"]);
        }
    }
}
