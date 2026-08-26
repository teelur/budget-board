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

    [HttpGet]
    [Authorize]
    [Route("link-candidates/{transactionID:guid}")]
    public async Task<IActionResult> ReadLinkCandidates(Guid transactionID, int dateWindowDays = 3)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            return Ok(
                await transactionService.ReadTransactionLinkCandidatesAsync(
                    parsedUserId,
                    transactionID,
                    dateWindowDays
                )
            );
        });
    }

    [HttpPost]
    [Authorize]
    [Route("link")]
    public async Task<IActionResult> Link([FromBody] TransactionLinkRequest request)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            return Ok(await transactionService.LinkTransactionsAsync(parsedUserId, request));
        });
    }

    [HttpPost]
    [Authorize]
    [Route("unlink/{transactionID:guid}")]
    public async Task<IActionResult> Unlink(Guid transactionID)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            return Ok(await transactionService.UnlinkTransactionAsync(parsedUserId, transactionID));
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

    [HttpPost]
    [Authorize]
    [Route("import/{jobId:guid}/cancel")]
    public async Task<IActionResult> CancelImport(Guid jobId)
    {
        return await HandleRequestAsync(async () =>
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            var importJob = await transactionImportService.RequestCancellationAsync(
                parsedUserId,
                jobId
            );
            return importJob is null ? NotFound() : Ok(importJob);
        });
    }
}
