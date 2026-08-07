using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Models.Widgets.NetWorthWidget;
using BudgetBoard.Utils;
using BudgetBoard.WebAPI.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NetWorthWidgetGroupController(
    ILogger<NetWorthWidgetGroupController> logger,
    UserManager<ApplicationUser> userManager,
    INetWorthWidgetGroupService netWorthWidgetGroupService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ApiControllerBase<NetWorthWidgetGroupController>(logger, logLocalizer, responseLocalizer)
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] NetWorthWidgetGroupCreateRequest newGroup)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await netWorthWidgetGroupService.CreateNetWorthWidgetGroupAsync(parsedUserId, newGroup);
            return Ok();
        });
    }

    [HttpPost]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> Reorder([FromBody] NetWorthWidgetGroupReorderRequest request)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await netWorthWidgetGroupService.ReorderNetWorthWidgetGroupsAsync(
                parsedUserId,
                request
            );
            return Ok();
        });
    }
}
