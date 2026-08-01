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
public class SyncController(
    ILogger<SyncController> logger,
    UserManager<ApplicationUser> userManager,
    ISyncService syncService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ApiControllerBase<SyncController>(logger, logLocalizer, responseLocalizer)
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Sync()
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            return Ok(await syncService.SyncAsync(parsedUserId));
        });
    }
}
