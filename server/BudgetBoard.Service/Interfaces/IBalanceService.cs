using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

public interface IBalanceService
{
    Task CreateBalancesAsync(Guid userGuid, IBalanceCreateRequest balance);
    Task<IEnumerable<IBalanceResponse>> ReadBalancesAsync(Guid userGuid, Guid accountId);
    Task UpdateBalanceAsync(Guid userGuid, IBalanceUpdateRequest updatedBalance);
    Task DeleteBalanceAsync(Guid userGuid, Guid guid);
    Task RestoreBalanceAsync(Guid userGuid, Guid guid);
}
