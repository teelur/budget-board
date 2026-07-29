using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models.Widgets.NetWorthWidget;
using BudgetBoard.WebAPI.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NetWorthWidgetLineController(
    ILogger<NetWorthWidgetLineController> logger,
    UserManager<ApplicationUser> userManager,
    INetWorthWidgetLineService netWorthWidgetLineService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ApiControllerBase<NetWorthWidgetLineController>(logger, logLocalizer, responseLocalizer)
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] NetWorthWidgetLineCreateRequest request)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await netWorthWidgetLineService.CreateNetWorthWidgetLineAsync(parsedUserId, request);
            return Ok();
        });
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] NetWorthWidgetLineUpdateRequest request)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await netWorthWidgetLineService.UpdateNetWorthWidgetLineAsync(parsedUserId, request);
            return Ok();
        });
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete(Guid widgetSettingsId, Guid lineId)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await netWorthWidgetLineService.DeleteNetWorthWidgetLineAsync(
                parsedUserId,
                widgetSettingsId,
                lineId
            );
            return Ok();
        });
    }

    [HttpPost]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> Reorder([FromBody] NetWorthWidgetLineReorderRequest request)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await netWorthWidgetLineService.ReorderNetWorthWidgetLinesAsync(parsedUserId, request);
            return Ok();
        });
    }
}
