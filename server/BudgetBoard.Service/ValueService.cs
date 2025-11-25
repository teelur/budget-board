using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class ValueService(
    ILogger<IValueService> logger,
    UserDataContext userDataContext,
    INowProvider nowProvider,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : IValueService
{
    private readonly ILogger<IValueService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly INowProvider _nowProvider = nowProvider;
    private readonly IStringLocalizer<ResponseStrings> _responseLocalizer = responseLocalizer;
    private readonly IStringLocalizer<LogStrings> _logLocalizer = logLocalizer;

    /// <inheritdoc />
    public async Task CreateValueAsync(Guid userGuid, IValueCreateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var asset = userData.Assets.FirstOrDefault(a => a.ID == request.AssetID);
        if (asset == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["ValueCreateAssetNotFoundLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["ValueCreateAssetNotFoundError"]
            );
        }

        var newValue = new Value()
        {
            DateTime = request.DateTime,
            Amount = request.Amount,
            AssetID = request.AssetID,
        };

        _userDataContext.Values.Add(newValue);
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IValueResponse>> ReadValuesAsync(Guid userGuid, Guid assetId)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var values = userData.Assets.Where(a => a.ID == assetId).SelectMany(a => a.Values);

        return values.Select(v => new ValueResponse(v)).ToList();
    }

    /// <inheritdoc />
    public async Task UpdateValueAsync(Guid userGuid, IValueUpdateRequest editedValue)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var value = userData
            .Assets.SelectMany(a => a.Values)
            .FirstOrDefault(v => v.ID == editedValue.ID);
        if (value == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["ValueUpdateNotFoundLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["ValueUpdateNotFoundError"]);
        }

        _userDataContext.Entry(value).CurrentValues.SetValues(editedValue);
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteValueAsync(Guid userGuid, Guid valueGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var value = userData
            .Assets.SelectMany(a => a.Values)
            .FirstOrDefault(v => v.ID == valueGuid);
        if (value == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["ValueDeleteNotFoundLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["ValueDeleteNotFoundError"]);
        }

        value.Deleted = _nowProvider.UtcNow;
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RestoreValueAsync(Guid userGuid, Guid valueGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var value = userData
            .Assets.SelectMany(a => a.Values)
            .FirstOrDefault(v => v.ID == valueGuid);
        if (value == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["ValueRestoreNotFoundLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["ValueRestoreNotFoundError"]);
        }

        value.Deleted = null;
        await _userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await _userDataContext
                .ApplicationUsers.Include(u => u.Assets)
                .ThenInclude(a => a.Values)
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["UserDataRetrievalError", ex.Message]);
            throw new BudgetBoardServiceException(_responseLocalizer["UserDataRetrievalError"]);
        }

        if (foundUser == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["InvalidUserError"]);
            throw new BudgetBoardServiceException(_responseLocalizer["InvalidUserError"]);
        }

        return foundUser;
    }
}
