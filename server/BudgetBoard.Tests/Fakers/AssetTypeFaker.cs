using Bogus;
using BudgetBoard.Database.Models;

namespace BudgetBoard.IntegrationTests.Fakers;

class AssetTypeFaker : Faker<AssetType>
{
    public AssetTypeFaker(Guid userID)
    {
        RuleFor(a => a.ID, f => f.Random.Guid())
            .RuleFor(a => a.Value, f => f.Random.String(20))
            .RuleFor(a => a.UserID, f => userID);
    }
}
