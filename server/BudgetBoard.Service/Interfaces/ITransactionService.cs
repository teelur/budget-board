using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

public interface ITransactionService
{
    Task CreateTransactionAsync(Guid userGuid, ITransactionCreateRequest transaction);
    Task<IEnumerable<ITransactionResponse>> ReadTransactionsAsync(Guid userGuid, int? year, int? month, bool getHidden, Guid guid = default);
    Task UpdateTransactionAsync(Guid userGuid, ITransactionUpdateRequest editedTransaction);
    Task DeleteTransactionAsync(Guid userGuid, Guid transactionID);
    Task RestoreTransactionAsync(Guid userGuid, Guid transactionID);
    Task SplitTransactionAsync(Guid userGuid, ITransactionSplitRequest transaction);
    Task ImportTransactionsAsync(Guid userGuid, ITransactionImportRequest transactionImportRequest);
}
