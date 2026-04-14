using Bogus;
using BudgetBoard.Database.Models;

namespace BudgetBoard.IntegrationTests.Fakers;

public class ValueFaker : Faker<Value>
{
    public ValueFaker()
    {
        RuleFor(a => a.ID, f => Guid.NewGuid())
            .RuleFor(a => a.Amount, f => f.Finance.Amount())
            .RuleFor(a => a.Date, f => DateOnly.FromDateTime(f.Date.Past()))
            .RuleFor(a => a.AssetID, f => Guid.NewGuid());
    }
}
