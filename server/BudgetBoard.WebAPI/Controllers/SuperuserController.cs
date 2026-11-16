using BudgetBoard.WebAPI.Models;
using BudgetBoard.WebAPI.Resources;
using BudgetBoard.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = SuperuserConstants.PolicyName)]
[EnableRateLimiting("superuser-management")]
public sealed class SuperuserController(
    ISuperuserService superuserService,
    UserManager<Database.Models.ApplicationUser> userManager,
    IStringLocalizer<ApiResponseStrings> localizer
) : ControllerBase
{
    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<ManagedUserResponse>>> ListUsers(
        CancellationToken cancellationToken
    )
    {
        return Ok(await superuserService.ListUsersAsync(cancellationToken));
    }

    [HttpGet("audit-events")]
    public async Task<ActionResult<AdminAuditEventsPageResponse>> ListAuditEvents(
        [FromQuery] AdminAuditEventsQuery query,
        CancellationToken cancellationToken
    )
    {
        return Ok(await superuserService.ListAuditEventsAsync(query, cancellationToken));
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateManagedUserRequest request,
        CancellationToken cancellationToken
    )
    {
        var actorId = GetActorId();
        if (actorId is null)
        {
            return Unauthorized();
        }

        var result = await superuserService.CreateUserAsync(
            request,
            actorId.Value,
            cancellationToken
        );
        return ToActionResult(result, StatusCodes.Status201Created);
    }

    [HttpPut("users/{userId:guid}/username")]
    public async Task<IActionResult> UpdateUsername(
        Guid userId,
        [FromBody] UpdateManagedUserUsernameRequest request,
        CancellationToken cancellationToken
    )
    {
        var actorId = GetActorId();
        if (actorId is null)
        {
            return Unauthorized();
        }

        return ToActionResult(
            await superuserService.UpdateUsernameAsync(
                userId,
                request,
                actorId.Value,
                cancellationToken
            )
        );
    }

    [HttpPost("users/{userId:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid userId, CancellationToken cancellationToken)
    {
        var actorId = GetActorId();
        if (actorId is null)
        {
            return Unauthorized();
        }

        return ToActionResult(
            await superuserService.ResetPasswordAsync(userId, actorId.Value, cancellationToken)
        );
    }

    [HttpPost("users/{userId:guid}/disable")]
    public async Task<IActionResult> Disable(Guid userId, CancellationToken cancellationToken) =>
        await SetDisabledAsync(userId, true, cancellationToken);

    [HttpPost("users/{userId:guid}/enable")]
    public async Task<IActionResult> Enable(Guid userId, CancellationToken cancellationToken) =>
        await SetDisabledAsync(userId, false, cancellationToken);

    [HttpPost("users/{userId:guid}/transfer-superuser")]
    public async Task<IActionResult> TransferSuperuser(
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        var actorId = GetActorId();
        if (actorId is null)
        {
            return Unauthorized();
        }

        return ToActionResult(
            await superuserService.TransferSuperuserAsync(userId, actorId.Value, cancellationToken)
        );
    }

    private async Task<IActionResult> SetDisabledAsync(
        Guid userId,
        bool disabled,
        CancellationToken cancellationToken
    )
    {
        var actorId = GetActorId();
        if (actorId is null)
        {
            return Unauthorized();
        }

        return ToActionResult(
            await superuserService.SetDisabledAsync(
                userId,
                actorId.Value,
                disabled,
                cancellationToken
            )
        );
    }

    private Guid? GetActorId()
    {
        var actorId = userManager.GetUserId(User);
        return Guid.TryParse(actorId, out var parsedActorId) ? parsedActorId : null;
    }

    private IActionResult ToActionResult(
        SuperuserOperationResult result,
        int successStatusCode = StatusCodes.Status200OK
    )
    {
        if (!result.Succeeded)
        {
            var statusCode = result.ErrorCode switch
            {
                "UserNotFound" => StatusCodes.Status404NotFound,
                "UsernameAlreadyTaken" or "EmailAlreadyTaken" => StatusCodes.Status409Conflict,
                "SuperuserUsernameChangeNotAllowed" => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status400BadRequest,
            };
            return Problem(
                detail: result.ErrorCode,
                statusCode: statusCode,
                title: localizer["AuthenticationManagementOperationFailed"].Value
            );
        }

        if (result.User is null)
        {
            return StatusCode(successStatusCode);
        }

        return StatusCode(
            successStatusCode,
            new ManagedUserOperationResponse(result.User, result.TemporaryPassword)
        );
    }
}
