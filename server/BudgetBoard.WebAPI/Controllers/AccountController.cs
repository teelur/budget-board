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
public class AccountController(
    ILogger<AccountController> logger,
    UserManager<ApplicationUser> userManager,
    IAccountService accountService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ApiControllerBase<AccountController>(logger, logLocalizer, responseLocalizer)
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] AccountCreateRequest account)
    {
        return await HandleRequestAsync(async () =>
        {
            await accountService.CreateAccountAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                account
            );
            return Ok();
        });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Read()
    {
        return await HandleRequestAsync(async () =>
        {
            return Ok(
                await accountService.ReadAccountsAsync(
                    new Guid(userManager.GetUserId(User) ?? string.Empty)
                )
            );
        });
    }

    [HttpGet("{guid}")]
    [Authorize]
    public async Task<IActionResult> Read(Guid guid)
    {
        try
        {
            return Ok(
                await _accountService.ReadAccountsAsync(
                    new Guid(_userManager.GetUserId(User) ?? string.Empty),
                    guid
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
    public async Task<IActionResult> Update([FromBody] AccountUpdateRequest editedAccount)
    {
        return await HandleRequestAsync(async () =>
        {
            await accountService.UpdateAccountAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                editedAccount
            );
            return Ok();
        });
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete(Guid guid, bool deleteTransactions = false)
    {
        return await HandleRequestAsync(async () =>
        {
            await accountService.DeleteAccountAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                guid,
                deleteTransactions
            );
            return Ok();
        });
    }

    [HttpPost]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> Restore(Guid guid)
    {
        return await HandleRequestAsync(async () =>
        {
            await accountService.RestoreAccountAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                guid
            );
            return Ok();
        });
    }

    [HttpPut]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> Order([FromBody] List<AccountIndexRequest> accounts)
    {
        return await HandleRequestAsync(async () =>
        {
            await accountService.OrderAccountsAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                accounts
            );
            return Ok();
        });
    }

    [HttpDelete]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> PermanentDelete(Guid guid)
    {
        return await HandleRequestAsync(async () =>
        {
            await accountService.PermanentlyDeleteAccountAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                guid
            );
            return Ok();
        });
    }
}
