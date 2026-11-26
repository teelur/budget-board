using System.Text.Json;
using BudgetBoard.Database.Models;
using BudgetBoard.WebAPI.Resources;
using BudgetBoard.WebAPI.Models;
using BudgetBoard.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationDestinationController(
    ILogger<NotificationDestinationController> logger,
    UserManager<ApplicationUser> userManager,
    INotificationDestinationService service,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ApiControllerBase<NotificationDestinationController>(
    logger,
    logLocalizer,
    responseLocalizer
)
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Read(CancellationToken cancellationToken)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            return Ok(await service.ReadAsync(parsedUserId, cancellationToken));
        });
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> ReadOne(Guid id, CancellationToken cancellationToken)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            var result = await service.ReadAsync(parsedUserId, id, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] NotificationDestinationRequest request,
        CancellationToken cancellationToken
    )
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            return await HandleConfigurationAsync(async () =>
            {
                var result = await service.CreateAsync(parsedUserId, request, cancellationToken);
                return CreatedAtAction(nameof(ReadOne), new { id = result.Id }, result);
            });
        });
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] NotificationDestinationRequest request,
        CancellationToken cancellationToken
    )
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            return await HandleConfigurationAsync(async () =>
            {
                var result = await service.UpdateAsync(parsedUserId, id, request, cancellationToken);
                return result is null ? NotFound() : Ok(result);
            });
        });
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            return await service.DeleteAsync(parsedUserId, id, cancellationToken)
                ? NoContent()
                : NotFound();
        });
    }

    [HttpPost("{id:guid}/test")]
    [Authorize]
    [EnableRateLimiting("notification-test")]
    public async Task<IActionResult> Test(Guid id, CancellationToken cancellationToken)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            var result = await service.TestAsync(parsedUserId, id, cancellationToken);
            if (result is null)
            {
                return NotFound();
            }
            return result.Value
                ? NoContent()
                : StatusCode(
                    StatusCodes.Status502BadGateway,
                    ResponseLocalizer["NotificationTestFailed"].Value
                );
        });
    }

    private async Task<IActionResult> HandleConfigurationAsync(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (NotificationDestinationLimitException)
        {
            return Conflict(ResponseLocalizer["NotificationDestinationLimitReached"].Value);
        }
        catch (ArgumentException)
        {
            return BadRequest(ResponseLocalizer["NotificationInvalidConfiguration"].Value);
        }
        catch (Exception exception) when (exception is InvalidOperationException or JsonException)
        {
            Logger.LogError(
                "{LogMessage}",
                LogLocalizer[
                    "NotificationConfigurationRequestFailedLog",
                    exception.GetType().Name
                ]
            );
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ResponseLocalizer["NotificationInvalidConfiguration"].Value
            );
        }
    }
}
