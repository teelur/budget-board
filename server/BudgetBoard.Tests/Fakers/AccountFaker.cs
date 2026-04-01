using Bogus;
using BudgetBoard.Database.Models;

namespace BudgetBoard.IntegrationTests.Fakers;

public class AccountFaker : Faker<Account>
{
    public AccountFaker(Guid userId)
    {
        RuleFor(a => a.ID, f => Guid.NewGuid())
            .RuleFor(a => a.Name, f => f.Finance.AccountName())
            .RuleFor(a => a.InstitutionID, f => Guid.NewGuid())
            .RuleFor(a => a.Type, f => f.Finance.TransactionType())
            .RuleFor(a => a.Subtype, f => f.Finance.TransactionType())
            .RuleFor(a => a.HideTransactions, f => false)
            .RuleFor(a => a.HideAccount, f => false)
            .RuleFor(a => a.UserID, f => userId);
    }
}
