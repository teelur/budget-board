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
public class LunchFlowController(
    ILogger<LunchFlowController> logger,
    UserManager<ApplicationUser> userManager,
    ILunchFlowService lunchFlowService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ApiControllerBase<LunchFlowController>(logger, logLocalizer, responseLocalizer)
{
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateApiKey(string apiKey)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await lunchFlowService.ConfigureApiKeyAsync(parsedUserId, apiKey);
            return Ok();
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> RemoveApiKey()
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await lunchFlowService.RemoveApiKeyAsync(parsedUserId);
            return Ok();
        });
    }
}
