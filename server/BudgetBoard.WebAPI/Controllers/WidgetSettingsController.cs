using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Utils;
using BudgetBoard.WebAPI.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WidgetSettingsController(
    ILogger<WidgetSettingsController> logger,
    UserManager<ApplicationUser> userManager,
    IWidgetSettingsService widgetSettingsService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ApiControllerBase<WidgetSettingsController>(logger, logLocalizer, responseLocalizer)
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] WidgetSettingsCreateRequest newWidgetSettings
    )
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await widgetSettingsService.CreateWidgetSettingsAsync(parsedUserId, newWidgetSettings);
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

            return Ok(await widgetSettingsService.ReadWidgetSettingsAsync(parsedUserId));
        });
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update(
        [FromBody] IEnumerable<WidgetSettingsUpdateRequest> editedWidgetSettings
    )
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await widgetSettingsService.UpdateWidgetSettingsAsync(
                parsedUserId,
                editedWidgetSettings
            );
            return Ok();
        });
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete(Guid widgetSettingsId)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await widgetSettingsService.DeleteWidgetSettingsAsync(parsedUserId, widgetSettingsId);
            return Ok();
        });
    }

    [HttpPost]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> ResetSmallScreenLayout()
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await widgetSettingsService.ResetSmallScreenToLargeScreenLayoutAsync(parsedUserId);
            return Ok();
        });
    }
}
