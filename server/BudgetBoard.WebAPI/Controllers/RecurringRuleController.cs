using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.WebAPI.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RecurringRuleController(
    ILogger<RecurringRuleController> logger,
    UserManager<ApplicationUser> userManager,
    IRecurringRuleService recurringRuleService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ApiControllerBase<RecurringRuleController>(logger, logLocalizer, responseLocalizer)
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] RecurringRuleCreateRequest request)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await recurringRuleService.CreateRecurringRuleAsync(parsedUserId, request);
            return Ok();
        });
    }

    [HttpPost("from-transaction/{transactionID:guid}")]
    [Authorize]
    public async Task<IActionResult> CreateFromTransaction(
        Guid transactionID,
        [FromBody] RecurringRuleCreateRequest request
    )
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await recurringRuleService.CreateRecurringRuleAsync(
                parsedUserId,
                request,
                transactionID
            );
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

            return Ok(await recurringRuleService.ReadRecurringRulesAsync(parsedUserId));
        });
    }

    [HttpGet("forecast")]
    [Authorize]
    public async Task<IActionResult> Forecast([FromQuery] DateOnly month)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            return Ok(await recurringRuleService.ReadForecastAsync(parsedUserId, month));
        });
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] RecurringRuleUpdateRequest request)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await recurringRuleService.UpdateRecurringRuleAsync(parsedUserId, request);
            return Ok();
        });
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete([FromQuery] Guid recurringRuleID)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await recurringRuleService.DeleteRecurringRuleAsync(parsedUserId, recurringRuleID);
            return Ok();
        });
    }

    [HttpPost("{recurringRuleID:guid}/transactions/{transactionID:guid}")]
    [Authorize]
    public async Task<IActionResult> AssignTransaction(Guid recurringRuleID, Guid transactionID)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await recurringRuleService.AssignTransactionAsync(
                parsedUserId,
                recurringRuleID,
                transactionID
            );
            return Ok();
        });
    }

    [HttpDelete("transactions/{transactionID:guid}")]
    [Authorize]
    public async Task<IActionResult> UnassignTransaction(Guid transactionID)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await recurringRuleService.UnassignTransactionAsync(parsedUserId, transactionID);
            return Ok();
        });
    }
}
