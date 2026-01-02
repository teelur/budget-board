using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

/// <inheritdoc />
public class SimpleFinOrganizationService(
    ILogger<ISimpleFinOrganizationService> logger,
    UserDataContext userDataContext,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : ISimpleFinOrganizationService
{
    /// <inheritdoc />
    public async Task CreateSimpleFinOrganizationAsync(
        Guid userGuid,
        ISimpleFinOrganizationCreateRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        if (userData.SimpleFinOrganizations.Any(i => i.Domain == request.Domain))
        {
            logger.LogError("{LogMessage}", logLocalizer["DuplicateOrganizationCreateLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["DuplicateOrganizationCreateError"]
            );
        }

        var newSimpleFinOrganization = new Database.Models.SimpleFinOrganization
        {
            Domain = request.Domain,
            SimpleFinUrl = request.SimpleFinUrl,
            Name = request.Name,
            Url = request.Url,
            SyncID = request.SyncID,
            UserID = userData.Id,
        };

        userDataContext.SimpleFinOrganizations.Add(newSimpleFinOrganization);
        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<
        IReadOnlyList<ISimpleFinOrganizationResponse>
    > ReadSimpleFinOrganizationsAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        return userData
            .SimpleFinOrganizations.Select(o => new SimpleFinOrganizationResponse(o))
            .ToList();
    }

    /// <inheritdoc />
    public async Task UpdateOrganizationAsync(
        Guid userGuid,
        ISimpleFinOrganizationUpdateRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var organizationToUpdate = userData.SimpleFinOrganizations.SingleOrDefault(a =>
            a.ID == request.ID
        );
        if (organizationToUpdate == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["SimpleFinOrganizationUpdateNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["SimpleFinOrganizationUpdateNotFoundError"]
            );
        }

        organizationToUpdate.Name = request.Name;
        organizationToUpdate.Domain = request.Domain;
        organizationToUpdate.SimpleFinUrl = request.SimpleFinUrl;
        organizationToUpdate.Url = request.Url;
        organizationToUpdate.SyncID = request.SyncID;

        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteOrganizationAsync(Guid userGuid, Guid organizationGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var organizationToDelete = userData.SimpleFinOrganizations.SingleOrDefault(o =>
            o.ID == organizationGuid
        );
        if (organizationToDelete == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["SimpleFinOrganizationDeleteNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["SimpleFinOrganizationDeleteNotFoundError"]
            );
        }

        userDataContext.SimpleFinOrganizations.Remove(organizationToDelete);
        await userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await userDataContext
                .ApplicationUsers.Include(u => u.SimpleFinOrganizations)
                .ThenInclude(i => i.Accounts)
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            logger.LogError("{LogMessage}", logLocalizer["UserDataRetrievalErrorLog", ex.Message]);
            throw new BudgetBoardServiceException(responseLocalizer["UserDataRetrievalError"]);
        }

        if (foundUser == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["InvalidUserErrorLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["InvalidUserError"]);
        }

        return foundUser;
    }
}
