using Bogus;
using BudgetBoard.Database.Models;

namespace BudgetBoard.IntegrationTests.Fakers;

public class BalanceFaker : Faker<Balance>
{
    public BalanceFaker(ICollection<Guid> accountIds)
    {
        RuleFor(b => b.ID, f => Guid.NewGuid())
            .RuleFor(b => b.Amount, f => f.Finance.Amount())
            .RuleFor(b => b.DateTime, f => f.Date.Past())
            .RuleFor(b => b.AccountID, f => f.PickRandom(accountIds));
    }
}
