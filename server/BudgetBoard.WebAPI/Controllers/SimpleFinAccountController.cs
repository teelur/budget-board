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
public class SimpleFinAccountController(
    ILogger<SimpleFinAccountController> logger,
    UserManager<ApplicationUser> userManager,
    ISimpleFinAccountService simpleFinAccountService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ApiControllerBase<SimpleFinAccountController>(logger, logLocalizer, responseLocalizer)
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

            return Ok(await simpleFinAccountService.ReadSimpleFinAccountsAsync(parsedUserId));
        });
    }

    [HttpPut]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> UpdateLinkedAccount(
        Guid simpleFinAccountGuid,
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

            await simpleFinAccountService.UpdateLinkedAccountAsync(
                parsedUserId,
                simpleFinAccountGuid,
                linkedAccountGuid
            );
            return Ok();
        });
    }

    [HttpPut]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> UpdateSyncStartDate(
        Guid simpleFinAccountGuid,
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

            await simpleFinAccountService.UpdateSimpleFinAccountSyncStartDateAsync(
                parsedUserId,
                simpleFinAccountGuid,
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
    public async Task<IActionResult> Delete(Guid simpleFinAccountGuid)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await simpleFinAccountService.DeleteSimpleFinAccountAsync(
                parsedUserId,
                simpleFinAccountGuid
            );
            return Ok();
        });
    }
}
