using Bogus;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;

namespace BudgetBoard.IntegrationTests.Fakers;

class UserSettingsFaker : Faker<UserSettings>
{
    public UserSettingsFaker(Guid userID)
    {
        RuleFor(u => u.ID, f => f.Random.Guid())
            .RuleFor(u => u.Currency, f => f.PickRandom(LocalizationHelpers.CurrencyCodes))
            .RuleFor(u => u.UserID, userID);
    }
}
