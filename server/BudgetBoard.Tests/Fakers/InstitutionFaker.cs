using Bogus;
using BudgetBoard.Database.Models;

namespace BudgetBoard.IntegrationTests.Fakers;

public class InstitutionFaker : Faker<Institution>
{
    public InstitutionFaker(Guid userID)
    {
        RuleFor(i => i.ID, f => f.Random.Guid())
            .RuleFor(i => i.Name, f => f.Company.CompanyName())
            .RuleFor(i => i.Index, f => f.Random.Int(0, 100))
            .RuleFor(i => i.UserID, f => userID);
    }
}
