using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

public interface IValueService
{
    Task CreateValueAsync(Guid userGuid, IValueCreateRequest value);
    Task<IEnumerable<IValueResponse>> ReadValuesAsync(Guid userGuid, Guid assetId);
    Task UpdateValueAsync(Guid userGuid, IValueUpdateRequest editedValue);
    Task DeleteValueAsync(Guid userGuid, Guid valueGuid);
    Task RestoreValueAsync(Guid userGuid, Guid valueGuid);
}
