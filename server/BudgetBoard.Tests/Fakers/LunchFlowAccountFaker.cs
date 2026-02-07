using Bogus;
using BudgetBoard.Database.Models;

namespace BudgetBoard.IntegrationTests.Fakers;

public class LunchFlowAccountFaker : Faker<LunchFlowAccount>
{
    public LunchFlowAccountFaker(Guid userId)
    {
        RuleFor(a => a.ID, f => Guid.NewGuid())
            .RuleFor(a => a.SyncID, f => f.Random.String(20))
            .RuleFor(a => a.Name, f => f.Finance.AccountName())
            .RuleFor(a => a.InstitutionName, f => f.Company.CompanyName())
            .RuleFor(a => a.InstitutionLogo, f => f.Internet.Url())
            .RuleFor(a => a.Provider, f => f.Random.String(10))
            .RuleFor(a => a.Currency, f => "USD")
            .RuleFor(a => a.Status, f => "active")
            .RuleFor(a => a.Balance, f => f.Finance.Amount(0, 10000))
            .RuleFor(
                a => a.BalanceDate,
                f => (int)new DateTimeOffset(f.Date.Recent()).ToUnixTimeSeconds()
            )
            .RuleFor(a => a.LastSync, f => f.Date.Recent().ToUniversalTime())
            .RuleFor(a => a.LinkedAccountId, f => null)
            .RuleFor(a => a.UserID, f => userId);
    }
}
