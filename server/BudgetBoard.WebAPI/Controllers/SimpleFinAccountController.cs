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
public class SimpleFinAccountController(
    ILogger<SimpleFinAccountController> logger,
    UserManager<ApplicationUser> userManager,
    ISimpleFinAccountService simpleFinAccountService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ControllerBase
{
    [HttpPut]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> UpdateLinkedAccount(
        Guid simpleFinAccountGuid,
        Guid? linkedAccountGuid
    )
    {
        try
        {
            await simpleFinAccountService.UpdateLinkedAccountAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                simpleFinAccountGuid,
                linkedAccountGuid
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
