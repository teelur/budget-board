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
public class NetWorthWidgetCategoryController(
    ILogger<NetWorthWidgetCategoryController> logger,
    UserManager<ApplicationUser> userManager,
    INetWorthWidgetCategoryService netWorthWidgetCategoryService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ApiControllerBase<NetWorthWidgetCategoryController>(logger, logLocalizer, responseLocalizer)
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] NetWorthWidgetCategoryCreateRequest request)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await netWorthWidgetCategoryService.CreateNetWorthWidgetCategoryAsync(
                parsedUserId,
                request
            );
            return Ok();
        });
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] NetWorthWidgetCategoryUpdateRequest request)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await netWorthWidgetCategoryService.UpdateNetWorthWidgetCategoryAsync(
                parsedUserId,
                request
            );
            return Ok();
        });
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete(Guid widgetSettingsId, Guid lineId, Guid categoryId)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await netWorthWidgetCategoryService.DeleteNetWorthWidgetCategoryAsync(
                parsedUserId,
                widgetSettingsId,
                lineId,
                categoryId
            );
            return Ok();
        });
    }
}
