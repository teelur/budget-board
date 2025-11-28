using Bogus;
using BudgetBoard.Database.Models;

namespace BudgetBoard.IntegrationTests.Fakers;

public class AssetFaker : Faker<Asset>
{
    public AssetFaker(Guid userID)
    {
        RuleFor(a => a.ID, f => Guid.NewGuid())
            .RuleFor(a => a.Name, f => f.Finance.AccountName())
            .RuleFor(a => a.PurchaseDate, f => f.Date.Past())
            .RuleFor(a => a.PurchasePrice, f => f.Finance.Amount())
            .RuleFor(a => a.SellDate, f => f.Date.Future())
            .RuleFor(a => a.SellPrice, f => f.Finance.Amount())
            .RuleFor(a => a.Hide, f => false)
            .RuleFor(a => a.Deleted, f => null)
            .RuleFor(a => a.Index, f => f.Random.Int(0, 100))
            .RuleFor(a => a.UserID, f => userID);
    }
}
