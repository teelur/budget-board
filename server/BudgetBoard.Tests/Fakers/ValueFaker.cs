using Bogus;
using BudgetBoard.Database.Models;

namespace BudgetBoard.IntegrationTests.Fakers;

public class ValueFaker : Faker<Value>
{
    public ValueFaker()
    {
        RuleFor(a => a.ID, f => Guid.NewGuid())
            .RuleFor(a => a.Amount, f => f.Finance.Amount())
            .RuleFor(a => a.DateTime, f => f.Date.Past())
            .RuleFor(a => a.Deleted, f => null)
            .RuleFor(a => a.AssetID, f => Guid.NewGuid());
    }
}
