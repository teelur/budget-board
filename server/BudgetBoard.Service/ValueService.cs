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
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : IValueService
{
    /// <inheritdoc />
    public async Task CreateValueAsync(Guid userGuid, IValueCreateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var asset = GetAssetById(userData, request.AssetID);

        var existingValue = asset.Values.FirstOrDefault(v => v.Date == request.Date);
        if (existingValue != null)
        {
            existingValue.Amount = request.Amount;
        }
        else
        {
            var newValue = new Value()
            {
                Date = request.Date,
                Amount = request.Amount,
                AssetID = request.AssetID,
            };
            userDataContext.Values.Add(newValue);
        }

        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IValueResponse>> ReadValuesAsync(Guid userGuid, Guid assetId)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var asset = GetAssetById(userData, assetId);

        return asset.Values.Select(v => new ValueResponse(v)).ToList();
    }

    /// <inheritdoc />
    public async Task UpdateValueAsync(Guid userGuid, IValueUpdateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var value = GetValueById(userData, request.ID);
        var asset = GetAssetById(userData, value.AssetID);

        if (request.Date.HasValue && asset.Values.Any(v => v.Date == request.Date.Value && v.ID != request.ID))
        {
            logger.LogError("{LogMessage}", logLocalizer["ValueDuplicateDateLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["ValueDuplicateDateError"]);
        }

        if (request.Amount.HasValue)
        {
            value.Amount = request.Amount.Value;
        }
        if (request.Date.HasValue)
        {
            value.Date = request.Date.Value;
        }

        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteValueAsync(Guid userGuid, Guid valueGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid);

        var value = userData
            .Assets.SelectMany(a => a.Values)
            .FirstOrDefault(v => v.ID == valueGuid);
        if (value == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["ValueDeleteNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["ValueDeleteNotFoundError"]);
        }

        userDataContext.Values.Remove(value);
        await userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(Guid id)
    {
        return await UserDataServiceHelper.GetCurrentUserAsync(
            userDataContext,
            logger,
            logLocalizer,
            responseLocalizer,
            id,
            users => users.Include(u => u.Assets).ThenInclude(a => a.Values)
        );
    }

    private Asset GetAssetById(ApplicationUser userData, Guid assetId)
    {
        var asset = userData.Assets.FirstOrDefault(a => a.ID == assetId);
        if (asset == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["ValueAssetNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["ValueAssetNotFoundError"]);
        }

        return asset;
    }

    private Value GetValueById(ApplicationUser userData, Guid valueId)
    {
        var value = userData.Assets.SelectMany(a => a.Values).FirstOrDefault(v => v.ID == valueId);
        if (value == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["ValueNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["ValueNotFoundError"]);
        }

        return value;
    }
}
