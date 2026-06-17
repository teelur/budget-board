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
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await accountService.CreateAccountAsync(parsedUserId, newAccount);
            return Ok();
        });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Read()
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            return Ok(await accountService.ReadAccountsAsync(parsedUserId));
        });
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] AccountUpdateRequest updatedAccount)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await accountService.UpdateAccountAsync(parsedUserId, updatedAccount);
            return Ok();
        });
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete(Guid accountId, bool deleteTransactions = false)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await accountService.DeleteAccountAsync(parsedUserId, accountId, deleteTransactions);
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
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }
            await accountService.RestoreAccountAsync(parsedUserId, accountId);
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
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await accountService.OrderAccountsAsync(parsedUserId, orderedAccounts);
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
