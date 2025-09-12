using Bogus;
using BudgetBoard.Database.Models;

namespace BudgetBoard.IntegrationTests.Fakers;

class UserSettingsFaker : Faker<UserSettings>
{
    public UserSettingsFaker()
    {
        RuleFor(u => u.ID, f => f.Random.Guid());
        RuleFor(u => u.Currency, f => f.Random.String());
    }
}
