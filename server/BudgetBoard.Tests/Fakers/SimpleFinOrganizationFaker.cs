using Bogus;
using BudgetBoard.Database.Models;

namespace BudgetBoard.IntegrationTests.Fakers;

public class SimpleFinOrganizationFaker : Faker<SimpleFinOrganization>
{
    public SimpleFinOrganizationFaker(Guid userId)
    {
        RuleFor(a => a.ID, f => Guid.NewGuid())
            .RuleFor(a => a.Domain, f => f.Finance.AccountName().ToLower() + ".com")
            .RuleFor(a => a.SimpleFinUrl, f => f.Finance.AccountName())
            .RuleFor(a => a.Name, f => f.Company.CompanyName())
            .RuleFor(a => a.Url, f => f.Internet.Url())
            .RuleFor(a => a.SyncID, f => f.Random.String(20))
            .RuleFor(a => a.UserID, f => userId);
    }
}
