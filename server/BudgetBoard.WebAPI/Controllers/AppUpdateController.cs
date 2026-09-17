using BudgetBoard.WebAPI.Models;
using BudgetBoard.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetBoard.WebAPI.Controllers;

[Route("api/app-update")]
[ApiController]
[Authorize]
public class AppUpdateController(IAppUpdateService appUpdateService, IConfiguration configuration)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(AppUpdateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AppUpdateResponse>> Get(
        [FromQuery] string? channel,
        CancellationToken cancellationToken
    )
    {
        if (!AppUpdateChannels.TryNormalize(channel, out var normalizedChannel))
        {
            return BadRequest("The channel must be either 'stable' or 'dev'.");
        }

        if (configuration.GetValue<bool>("DISABLE_UPDATE_CHECK"))
        {
            return NoContent();
        }

        var update = await appUpdateService.GetLatestAsync(normalizedChannel, cancellationToken);

        return update is null ? NoContent() : Ok(update);
    }
}
