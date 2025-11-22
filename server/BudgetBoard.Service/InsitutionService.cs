using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class InstitutionService(
    ILogger<IInstitutionService> logger,
    UserDataContext userDataContext,
    INowProvider nowProvider
) : IInstitutionService
{
    private readonly ILogger<IInstitutionService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly INowProvider _nowProvider = nowProvider;

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
            _logger.LogError("Attempted to create an institution with a duplicate name.");
            throw new BudgetBoardServiceException("An institution with this name already exists.");
        }

        var institution = new Institution { Name = request.Name, UserID = userGuid };

        userData.Institutions.Add(institution);
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
                _logger.LogError("Attempt to access non-existent institution.");
                throw new BudgetBoardServiceException(
                    "The institution you are trying to access does not exist."
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
            _logger.LogError("Attempt to update non-existent institution.");
            throw new BudgetBoardServiceException(
                "The institution you are trying to update does not exist."
            );
        }

        if (string.IsNullOrEmpty(request.Name))
        {
            _logger.LogError("Attempt to update institution with empty name.");
            throw new BudgetBoardServiceException("Institution name cannot be empty.");
        }

        if (
            !institution.Name.Equals(request.Name, StringComparison.InvariantCultureIgnoreCase)
            && userData.Institutions.Any(i =>
                i.Name.Equals(request.Name, StringComparison.InvariantCultureIgnoreCase)
            )
        )
        {
            _logger.LogError("Attempted to rename an institution to a duplicate name.");
            throw new BudgetBoardServiceException("An institution with this name already exists.");
        }

        institution.Name = request.Name;

        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteInstitutionAsync(Guid userGuid, Guid id, bool deleteTransactions)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var institution = userData.Institutions.FirstOrDefault(i => i.ID == id);
        if (institution == null)
        {
            _logger.LogError("Attempt to delete non-existent institution.");
            throw new BudgetBoardServiceException(
                "The institution you are trying to delete does not exist."
            );
        }

        if (deleteTransactions)
        {
            foreach (var transaction in institution.Accounts.SelectMany(a => a.Transactions))
            {
                transaction.Deleted = _nowProvider.UtcNow;
            }
        }

        userData.Institutions.Remove(institution);
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
                _logger.LogError("Attempt to order non-existent institution.");
                throw new BudgetBoardServiceException(
                    "The institution you are trying to order does not exist."
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
