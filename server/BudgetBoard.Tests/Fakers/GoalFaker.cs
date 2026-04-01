using Bogus;
using BudgetBoard.Database.Models;

namespace BudgetBoard.IntegrationTests.Fakers;

class GoalFaker : Faker<Goal>
{
    public GoalFaker(Guid userId)
    {
        RuleFor(g => g.ID, f => Guid.NewGuid())
            .RuleFor(g => g.Name, f => f.Lorem.Word())
            .RuleFor(g => g.CompleteDate, f => f.Date.Future())
            .RuleFor(g => g.Amount, f => f.Finance.Amount())
            .RuleFor(g => g.InitialAmount, f => f.Finance.Amount())
            .RuleFor(g => g.MonthlyContribution, f => f.Finance.Amount())
            .RuleFor(g => g.UserID, f => userId);
    }
}
