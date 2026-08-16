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
public class TransactionController(
    ILogger<TransactionController> logger,
    UserManager<ApplicationUser> userManager,
    ITransactionService transactionService,
    ITransactionImportService transactionImportService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ApiControllerBase<TransactionController>(logger, logLocalizer, responseLocalizer)
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] TransactionCreateRequest newTransaction)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await transactionService.CreateTransactionAsync(parsedUserId, newTransaction);
            return Ok();
        });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Read(
        int? year,
        int? month,
        bool includeHiddenAccounts,
        bool includeHiddenCategory,
        bool includeDeleted
    )
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            return Ok(
                await transactionService.ReadTransactionsAsync(
                    parsedUserId,
                    year,
                    month,
                    includeHiddenAccounts,
                    includeHiddenCategory,
                    includeDeleted
                )
            );
        });
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update(
        [FromBody] IEnumerable<TransactionUpdateRequest> updatedTransactions
    )
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await transactionService.UpdateTransactionsAsync(parsedUserId, updatedTransactions);
            return Ok();
        });
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete([FromBody] IEnumerable<Guid> transactionIds)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await transactionService.DeleteTransactionsAsync(parsedUserId, transactionIds);
            return Ok();
        });
    }

    [HttpPost]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> Restore([FromBody] IEnumerable<Guid> transactionIds)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await transactionService.RestoreTransactionsAsync(parsedUserId, transactionIds);
            return Ok();
        });
    }

    [HttpPost]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> Split([FromBody] TransactionSplitRequest splitTransaction)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            await transactionService.SplitTransactionAsync(parsedUserId, splitTransaction);
            return Ok();
        });
    }

    [HttpPost]
    [Authorize]
    [Route("[action]")]
    public async Task<IActionResult> Import(
        [FromBody] TransactionImportRequest importedTransactions
    )
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault();
            var importJob = await transactionImportService.EnqueueAsync(
                parsedUserId,
                importedTransactions,
                idempotencyKey
            );

            return AcceptedAtAction(
                nameof(ReadImportStatus),
                new { jobId = importJob.ID },
                importJob
            );
        });
    }

    [HttpGet]
    [Authorize]
    [Route("import/{jobId:guid}")]
    public async Task<IActionResult> ReadImportStatus(Guid jobId)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            var importJob = await transactionImportService.ReadStatusAsync(parsedUserId, jobId);
            return importJob is null ? NotFound() : Ok(importJob);
        });
    }
}
