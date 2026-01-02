using Bogus;
using BudgetBoard.Database.Models;

namespace BudgetBoard.IntegrationTests.Fakers;

public class SimpleFinAccountFaker : Faker<SimpleFinAccount>
{
    public SimpleFinAccountFaker(Guid userId)
    {
        RuleFor(a => a.ID, f => Guid.NewGuid())
            .RuleFor(a => a.SyncID, f => f.Random.String(20))
            .RuleFor(a => a.Name, f => f.Finance.AccountName())
            .RuleFor(a => a.Currency, f => "USD")
            .RuleFor(a => a.Balance, f => f.Finance.Amount(0, 10000))
            .RuleFor(a => a.BalanceDate, f => f.Random.Number(0, 10000))
            .RuleFor(a => a.LastSync, f => f.Date.Recent().ToUniversalTime())
            .RuleFor(a => a.OrganizationId, f => Guid.NewGuid())
            .RuleFor(a => a.UserID, f => userId);
    }
}
