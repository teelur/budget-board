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
    private readonly ILogger<IInstitutionService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly INowProvider _nowProvider = nowProvider;
    private readonly IStringLocalizer<ResponseStrings> _responseLocalizer = responseLocalizer;
    private readonly IStringLocalizer<LogStrings> _logLocalizer = logLocalizer;

    /// <inheritdoc />
    public async Task CreateInstitutionAsync(Guid userGuid, IInstitutionCreateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        if (
            userData.Institutions.Any(i =>
                i.Name.Equals(request.Name, StringComparison.InvariantCultureIgnoreCase)
            )
        )
        {
            _logger.LogError("{LogMessage}", _logLocalizer["InstitutionCreateDuplicateNameLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["InstitutionCreateDuplicateNameError"]
            );
        }

        var institution = new Institution { Name = request.Name, UserID = userGuid };

        _userDataContext.Institutions.Add(institution);
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IInstitutionResponse>> ReadInstitutionsAsync(
        Guid userGuid,
        Guid guid = default
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        if (guid != default)
        {
            var insitution = userData.Institutions.FirstOrDefault(i => i.ID == guid);
            if (insitution == null)
            {
                _logger.LogError("{LogMessage}", _logLocalizer["InstitutionNotFoundLog"]);
                throw new BudgetBoardServiceException(
                    _responseLocalizer["InstitutionNotFoundError"]
                );
            }

            return [new InstitutionResponse(insitution)];
        }

        return userData.Institutions.Select(i => new InstitutionResponse(i)).ToList();
    }

    /// <inheritdoc />
    public async Task UpdateInstitutionAsync(Guid userGuid, IInstitutionUpdateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var institution = userData.Institutions.FirstOrDefault(i => i.ID == request.ID);
        if (institution == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["InstitutionUpdateNotFoundLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["InstitutionUpdateNotFoundError"]
            );
        }

        if (string.IsNullOrEmpty(request.Name))
        {
            _logger.LogError("{LogMessage}", _logLocalizer["InstitutionUpdateEmptyNameLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["InstitutionUpdateEmptyNameError"]
            );
        }

        if (
            !institution.Name.Equals(request.Name, StringComparison.InvariantCultureIgnoreCase)
            && userData.Institutions.Any(i =>
                i.Name.Equals(request.Name, StringComparison.InvariantCultureIgnoreCase)
            )
        )
        {
            _logger.LogError("{LogMessage}", _logLocalizer["InstitutionUpdateDuplicateNameLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["InstitutionUpdateDuplicateNameError"]
            );
        }

        _userDataContext.Entry(institution).CurrentValues.SetValues(request);
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteInstitutionAsync(Guid userGuid, Guid id, bool deleteTransactions)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var institution = userData.Institutions.FirstOrDefault(i => i.ID == id);
        if (institution == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["InstitutionDeleteNotFoundLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["InstitutionDeleteNotFoundError"]
            );
        }

        if (deleteTransactions)
        {
            foreach (var transaction in institution.Accounts.SelectMany(a => a.Transactions))
            {
                transaction.Deleted = _nowProvider.UtcNow;
            }
        }

        institution.Deleted = _nowProvider.UtcNow;
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task OrderInstitutionsAsync(
        Guid userGuid,
        IEnumerable<IInstitutionIndexRequest> orderedInstitutions
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        foreach (var institution in orderedInstitutions)
        {
            var insitution = userData.Institutions.FirstOrDefault(i => i.ID == institution.ID);
            if (insitution == null)
            {
                _logger.LogError("{LogMessage}", _logLocalizer["InstitutionOrderNotFoundLog"]);
                throw new BudgetBoardServiceException(
                    _responseLocalizer["InstitutionOrderNotFoundError"]
                );
            }

            insitution.Index = institution.Index;
        }

        await _userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await _userDataContext
                .ApplicationUsers.Include(u => u.Institutions)
                .ThenInclude(i => i.Accounts)
                .ThenInclude(a => a.Balances)
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["UserDataRetrievalErrorLog", ex.Message]
            );
            throw new BudgetBoardServiceException(_responseLocalizer["UserDataRetrievalError"]);
        }

        if (foundUser == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["InvalidUserErrorLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["InvalidUserError"]);
        }

        return foundUser;
    }
}
