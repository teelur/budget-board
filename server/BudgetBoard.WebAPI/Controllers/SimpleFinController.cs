using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.WebAPI.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class SimpleFinController(
    ILogger<SimpleFinController> logger,
    UserManager<ApplicationUser> userManager,
    ISimpleFinService simpleFinService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ApiControllerBase<SimpleFinController>(logger, logLocalizer, responseLocalizer)
{
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateAccessToken(string setupToken)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await simpleFinService.ConfigureAccessTokenAsync(parsedUserId, setupToken);
            return Ok();
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> RemoveAccessToken()
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await simpleFinService.RemoveAccessTokenAsync(parsedUserId);
            return Ok();
        });
    }
}
