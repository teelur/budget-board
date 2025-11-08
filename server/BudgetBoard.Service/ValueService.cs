using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class ValueService(
    ILogger<IValueService> logger,
    UserDataContext userDataContext,
    INowProvider nowProvider
) : IValueService
{
    private readonly ILogger<IValueService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly INowProvider _nowProvider = nowProvider;

    public async Task CreateValueAsync(Guid userGuid, IValueCreateRequest value)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var asset = userData.Assets.FirstOrDefault(a => a.ID == value.AssetID);
        if (asset == null)
        {
            _logger.LogError("Attempt to add value to asset that does not exist.");
            throw new BudgetBoardServiceException(
                "The asset you are trying to add a value to does not exist."
            );
        }

        var newValue = new Value()
        {
            DateTime = value.DateTime,
            Amount = value.Amount,
            AssetID = value.AssetID,
        };

        asset.Values.Add(newValue);
        await _userDataContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<IValueResponse>> ReadValuesAsync(Guid userGuid, Guid assetId)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var values = userData.Assets.SelectMany(a => a.Values).Where(v => v.AssetID == assetId);

        return values.Select(v => new ValueResponse(v));
    }

    public async Task UpdateValueAsync(Guid userGuid, IValueUpdateRequest editedValue)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var value = userData
            .Assets.SelectMany(a => a.Values)
            .FirstOrDefault(v => v.ID == editedValue.ID);
        if (value == null)
        {
            _logger.LogError("Attempt to update value that does not exist.");
            throw new BudgetBoardServiceException(
                "The value you are trying to update does not exist."
            );
        }

        value.DateTime = editedValue.DateTime;
        value.Amount = editedValue.Amount;

        await _userDataContext.SaveChangesAsync();
    }

    public async Task DeleteValueAsync(Guid userGuid, Guid valueGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var value = userData
            .Assets.SelectMany(a => a.Values)
            .FirstOrDefault(v => v.ID == valueGuid);
        if (value == null)
        {
            _logger.LogError("Attempt to delete value that does not exist.");
            throw new BudgetBoardServiceException(
                "The value you are trying to delete does not exist."
            );
        }

        value.Deleted = _nowProvider.UtcNow;

        await _userDataContext.SaveChangesAsync();
    }

    public async Task RestoreValueAsync(Guid userGuid, Guid valueGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var value = userData
            .Assets.SelectMany(a => a.Values)
            .FirstOrDefault(v => v.ID == valueGuid);
        if (value == null)
        {
            _logger.LogError("Attempt to restore value that does not exist.");
            throw new BudgetBoardServiceException(
                "The value you are trying to restore does not exist."
            );
        }

        value.Deleted = null;

        await _userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        List<ApplicationUser> users;
        ApplicationUser? foundUser;
        try
        {
            users = await _userDataContext
                .ApplicationUsers.Include(u => u.Assets)
                .ThenInclude(a => a.Values)
                .AsSplitQuery()
                .ToListAsync();
            foundUser = users.FirstOrDefault(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "An error occurred while retrieving the user data: {ExceptionMessage}",
                ex.Message
            );
            throw new BudgetBoardServiceException(
                "An error occurred while retrieving the user data."
            );
        }

        if (foundUser == null)
        {
            _logger.LogError("Attempt to create an account for an invalid user.");
            throw new BudgetBoardServiceException("Provided user not found.");
        }

        return foundUser;
    }
}
