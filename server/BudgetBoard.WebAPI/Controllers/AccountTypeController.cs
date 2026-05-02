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
public class AccountTypeController(
    ILogger<AccountTypeController> logger,
    UserManager<ApplicationUser> userManager,
    IAccountTypeService accountTypeService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] AccountTypeCreateRequest accountType)
    {
        try
        {
            await accountTypeService.CreateAccountTypeAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                accountType
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
                await accountTypeService.ReadAccountTypesAsync(
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
    public async Task<IActionResult> Update([FromBody] AccountTypeUpdateRequest accountType)
    {
        try
        {
            await accountTypeService.UpdateAccountTypeAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                accountType
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
    public async Task<IActionResult> Delete(Guid guid)
    {
        try
        {
            await accountTypeService.DeleteAccountTypeAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                guid
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
