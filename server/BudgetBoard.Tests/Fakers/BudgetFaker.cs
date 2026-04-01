using Bogus;
using BudgetBoard.Database.Models;

namespace BudgetBoard.IntegrationTests.Fakers;

class BudgetFaker : Faker<Budget>
{
    public BudgetFaker(Guid userID)
    {
        RuleFor(b => b.ID, f => Guid.NewGuid())
            .RuleFor(b => b.Date, f => f.Date.Between(DateTime.Now.AddMonths(-2), DateTime.Now))
            .RuleFor(b => b.Category, f => f.Finance.AccountName())
            .RuleFor(b => b.Limit, f => f.Finance.Amount())
            .RuleFor(b => b.UserID, f => userID);
    }
}
