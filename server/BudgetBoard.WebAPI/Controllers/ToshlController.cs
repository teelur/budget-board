using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Utils;
using BudgetBoard.WebAPI.Resources;
using BudgetBoard.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class ToshlController(
    ILogger<ToshlController> logger,
    UserManager<ApplicationUser> userManager,
    IToshlService toshlService,
    IToshlFullSyncQueue toshlFullSyncQueue,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ControllerBase
{
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateAccessToken(string accessToken)
    {
        try
        {
            var userGuid = new Guid(userManager.GetUserId(User) ?? string.Empty);
            await toshlService.ConfigureAccessTokenAsync(userGuid, accessToken);
            await toshlFullSyncQueue.QueueAsync(userGuid);
            return Accepted();
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message, bbex.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{LogMessage}", logLocalizer["UnexpectedErrorLog"]);
            return Helpers.BuildErrorResponse(BuildDetailedErrorMessage(ex));
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> RemoveAccessToken()
    {
        try
        {
            await toshlService.RemoveAccessTokenAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty)
            );
            return Ok();
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message, bbex.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{LogMessage}", logLocalizer["UnexpectedErrorLog"]);
            return Helpers.BuildErrorResponse(BuildDetailedErrorMessage(ex));
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Sync()
    {
        try
        {
            await toshlFullSyncQueue.QueueAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty)
            );
            return Accepted();
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message, bbex.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{LogMessage}", logLocalizer["UnexpectedErrorLog"]);
            return Helpers.BuildErrorResponse(BuildDetailedErrorMessage(ex));
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> CategoryMappings()
    {
        try
        {
            return Ok(
                await toshlService.ReadCategoryMappingsAsync(
                    new Guid(userManager.GetUserId(User) ?? string.Empty)
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
            return Helpers.BuildErrorResponse(BuildDetailedErrorMessage(ex));
        }
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> CategoryMappings(
        [FromBody] ToshlCategoryMappingsUpdateRequest request
    )
    {
        try
        {
            await toshlService.UpdateCategoryMappingsAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                request
            );
            return Ok();
        }
        catch (BudgetBoardServiceException bbex)
        {
            return Helpers.BuildErrorResponse(bbex.Message, bbex.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{LogMessage}", logLocalizer["UnexpectedErrorLog"]);
            return Helpers.BuildErrorResponse(BuildDetailedErrorMessage(ex));
        }
    }

    private static string BuildDetailedErrorMessage(Exception ex)
    {
        var root = ex.GetBaseException();
        return $"{root.GetType().Name}: {root.Message}";
    }
}
