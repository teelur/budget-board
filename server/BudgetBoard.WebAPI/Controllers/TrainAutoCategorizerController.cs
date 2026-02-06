using BudgetBoard.Database.Models;
using BudgetBoard.Service;
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
public class TrainAutoCategorizerController(
    ILogger<TrainAutoCategorizerController> logger,
    UserManager<ApplicationUser> userManager,
    IAutomaticTransactionCategorizerService automaticTransactionCategorizerService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Train([FromBody] TrainAutoCategorizerRequest trainRequest)
    {
        try
        {
            await automaticTransactionCategorizerService.TrainCategorizerAsync(new Guid(userManager.GetUserId(User) ?? string.Empty), trainRequest);
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

        return Ok();
    }
}
