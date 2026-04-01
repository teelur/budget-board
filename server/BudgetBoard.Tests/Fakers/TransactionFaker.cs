using Bogus;
using BudgetBoard.Database.Models;

namespace BudgetBoard.IntegrationTests.Fakers;

public class TransactionFaker : Faker<Transaction>
{
    public ICollection<Guid> AccountIds { get; set; }

    public TransactionFaker(ICollection<Guid> accountIds)
    {
        AccountIds = accountIds;

        RuleFor(t => t.ID, f => Guid.NewGuid())
            .RuleFor(t => t.SyncID, f => f.Random.String(20))
            .RuleFor(t => t.Amount, f => f.Finance.Amount())
            .RuleFor(t => t.Date, f => f.Date.Past())
            .RuleFor(t => t.Category, f => f.Random.String(10))
            .RuleFor(t => t.Subcategory, f => f.Random.String(10))
            .RuleFor(t => t.MerchantName, f => f.Random.String(10))
            .RuleFor(t => t.Source, f => f.Random.String(10))
            .RuleFor(t => t.AccountID, f => f.PickRandom(AccountIds));
    }
}
