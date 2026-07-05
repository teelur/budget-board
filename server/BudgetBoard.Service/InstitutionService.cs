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

public class InstitutionService(
    ILogger<IInstitutionService> logger,
    UserDataContext userDataContext,
    INowProvider nowProvider,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : IInstitutionService
{
    /// <inheritdoc />
    public async Task CreateInstitutionAsync(Guid userGuid, IInstitutionCreateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        ValidateInstitutionName(userData.Institutions, request.Name);

        var institution = new Institution { Name = request.Name, UserID = userGuid };

        userDataContext.Institutions.Add(institution);
        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IInstitutionResponse>> ReadInstitutionsAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        return userData.Institutions.Select(i => new InstitutionResponse(i)).ToList();
    }

    /// <inheritdoc />
    public async Task UpdateInstitutionAsync(Guid userGuid, IInstitutionUpdateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var institution = GetInstitutionById(userData.Institutions, request.ID);
        ValidateInstitutionName(
            userData.Institutions.Where(i => i.ID != request.ID).ToList(),
            request.Name
        );

        institution.Name = request.Name;

        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteInstitutionAsync(
        Guid userGuid,
        Guid id,
        bool deleteTransactions,
        bool deferSave = false
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var institution = GetInstitutionById(userData.Institutions, id);

        if (deleteTransactions)
        {
            var transactionsToDelete = institution
                .Accounts.SelectMany(a => a.Transactions)
                .Where(t => t.Deleted == null)
                .ToList();
            transactionsToDelete.ForEach(t => t.Deleted = nowProvider.UtcNow);
        }

        var accountsToDelete = institution.Accounts.Where(a => a.Deleted == null).ToList();
        accountsToDelete.ForEach(a => a.Deleted = nowProvider.UtcNow);

        institution.Deleted = nowProvider.UtcNow;
        institution.Index = 0;
        if (!deferSave)
        {
            await userDataContext.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task OrderInstitutionsAsync(
        Guid userGuid,
        IEnumerable<IInstitutionIndexRequest> request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        foreach (var requestInstitution in request)
        {
            var institution = GetInstitutionById(userData.Institutions, requestInstitution.ID);
            institution.Index = requestInstitution.Index;
        }

        await userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        return await UserDataServiceHelper.GetCurrentUserAsync(
            userDataContext,
            logger,
            logLocalizer,
            responseLocalizer,
            id,
            users =>
                users
                    .Include(u => u.Institutions)
                    .ThenInclude(i => i.Accounts)
                    .ThenInclude(a => a.Balances)
        );
    }

    private void ValidateInstitutionName(ICollection<Institution> institutions, string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            logger.LogError("{LogMessage}", logLocalizer["InstitutionEmptyNameLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["InstitutionEmptyNameError"]);
        }
        if (institutions.Any(i => i.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
        {
            logger.LogError("{LogMessage}", logLocalizer["InstitutionDuplicateNameLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["InstitutionDuplicateNameError"]
            );
        }
    }

    private Institution GetInstitutionById(ICollection<Institution> institutions, Guid id)
    {
        var institution = institutions.FirstOrDefault(i => i.ID == id);
        if (institution == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["InstitutionNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["InstitutionNotFoundError"]);
        }
        return institution;
    }
}
