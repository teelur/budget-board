using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
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
    public async Task<IActionResult> Create([FromBody] AccountCreateRequest newAccount)
    {
        return await HandleRequestAsync(async () =>
        {
            await accountService.CreateAccountAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                newAccount
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

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] AccountUpdateRequest updatedAccount)
    {
        return await HandleRequestAsync(async () =>
        {
            await accountService.UpdateAccountAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                updatedAccount
            );
            return Ok();
        });
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete(Guid accountId, bool deleteTransactions = false)
    {
        return await HandleRequestAsync(async () =>
        {
            await accountService.DeleteAccountAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                accountId,
                deleteTransactions
            );
            return Ok();
        });
    }

    [HttpPost]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> Restore(Guid accountId)
    {
        return await HandleRequestAsync(async () =>
        {
            await accountService.RestoreAccountAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                accountId
            );
            return Ok();
        });
    }

    [HttpPut]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> Order([FromBody] List<AccountIndexRequest> orderedAccounts)
    {
        return await HandleRequestAsync(async () =>
        {
            await accountService.OrderAccountsAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                orderedAccounts
            );
            return Ok();
        });
    }

    [HttpDelete]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> PermanentDelete(Guid accountId)
    {
        return await HandleRequestAsync(async () =>
        {
            await accountService.PermanentlyDeleteAccountAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                accountId
            );
            return Ok();
        });
    }
}
