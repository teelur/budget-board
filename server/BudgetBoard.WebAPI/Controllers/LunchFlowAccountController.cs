using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.WebAPI.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LunchFlowAccountController(
    ILogger<LunchFlowAccountController> logger,
    UserManager<ApplicationUser> userManager,
    ILunchFlowAccountService lunchFlowAccountService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ApiControllerBase<LunchFlowAccountController>(logger, logLocalizer, responseLocalizer)
{
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

            return Ok(await lunchFlowAccountService.ReadLunchFlowAccountsAsync(parsedUserId));
        });
    }

    [HttpPut]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> UpdateLinkedAccount(
        Guid lunchFlowAccountGuid,
        Guid? linkedAccountGuid
    )
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await lunchFlowAccountService.UpdateLinkedAccountAsync(
                parsedUserId,
                lunchFlowAccountGuid,
                linkedAccountGuid
            );
            return Ok();
        });
    }

    [HttpPut]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> UpdateSyncStartDate(
        Guid lunchFlowAccountGuid,
        string? syncStartDate
    )
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await lunchFlowAccountService.UpdateLunchFlowAccountSyncStartDateAsync(
                parsedUserId,
                lunchFlowAccountGuid,
                syncStartDate != null
                    ? DateOnly.ParseExact(
                        syncStartDate,
                        "yyyy-MM-dd",
                        System.Globalization.CultureInfo.InvariantCulture
                    )
                    : null
            );
            return Ok();
        });
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete(Guid lunchFlowAccountGuid)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await lunchFlowAccountService.DeleteLunchFlowAccountAsync(
                parsedUserId,
                lunchFlowAccountGuid
            );
            return Ok();
        });
    }
}
