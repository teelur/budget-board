using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

public interface ITransactionImportService
{
    Task<TransactionImportJobResponse> EnqueueAsync(
        Guid userGuid,
        ITransactionImportRequest request,
        string? idempotencyKey = null
    );

    Task<TransactionImportJobResponse?> ReadStatusAsync(Guid userGuid, Guid jobId);

    Task<TransactionImportJobResponse?> RequestCancellationAsync(Guid userGuid, Guid jobId);

    Task<bool> ProcessNextAsync(CancellationToken cancellationToken);
}
