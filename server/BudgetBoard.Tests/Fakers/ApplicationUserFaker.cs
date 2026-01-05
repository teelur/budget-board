using Bogus;
using BudgetBoard.Database.Models;

namespace BudgetBoard.IntegrationTests.Fakers;

class ApplicationUserFaker : Faker<ApplicationUser>
{
    public ApplicationUserFaker()
    {
        RuleFor(u => u.Id, f => f.Random.Guid());
        RuleFor(u => u.SimpleFinAccessToken, f => f.Random.String());
        RuleFor(u => u.LastSync, f => f.Date.Past());
    }
}
