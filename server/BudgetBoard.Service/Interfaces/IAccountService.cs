using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

public interface IAccountService
{
    Task CreateAccountAsync(Guid userGuid, IAccountCreateRequest account);
    Task<IEnumerable<IAccountResponse>> ReadAccountsAsync(
        Guid userGuid,
        Guid accountGuid = default
    );
    Task UpdateAccountAsync(Guid userGuid, IAccountUpdateRequest editedAccount);
    Task DeleteAccountAsync(Guid userGuid, Guid guid, bool deleteTransactions = false);
    Task RestoreAccountAsync(Guid userGuid, Guid guid, bool restoreTransactions = false);
    Task OrderAccountsAsync(Guid userGuid, IEnumerable<IAccountIndexRequest> orderedAccounts);
}
