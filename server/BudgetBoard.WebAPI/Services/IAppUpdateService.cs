using BudgetBoard.WebAPI.Models;

namespace BudgetBoard.WebAPI.Services;

public interface IAppUpdateService
{
    Task<AppUpdateResponse?> GetLatestAsync(string channel, CancellationToken cancellationToken);
}
