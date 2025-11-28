using Bogus;
using BudgetBoard.Database.Models;

namespace BudgetBoard.IntegrationTests.Fakers;

class TransactionCategoryFaker : Faker<Category>
{
    public TransactionCategoryFaker(Guid userID)
    {
        RuleFor(c => c.ID, f => f.Random.Guid())
            .RuleFor(c => c.Value, f => f.Random.String(20))
            .RuleFor(c => c.Parent, f => f.Random.String(20))
            .RuleFor(c => c.UserID, f => userID);
    }
}
