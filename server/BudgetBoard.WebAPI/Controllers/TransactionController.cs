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
public class TransactionController(
    ILogger<TransactionController> logger,
    UserManager<ApplicationUser> userManager,
    ITransactionService transactionService,
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
        bool includeHidden,
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
                    includeHidden,
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

            await transactionService.ImportTransactionsAsync(parsedUserId, importedTransactions);
            return Ok();
        });
    }
}
