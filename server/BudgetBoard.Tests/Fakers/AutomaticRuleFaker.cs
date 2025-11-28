using Bogus;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;

namespace BudgetBoard.IntegrationTests.Fakers;

public class AutomaticRuleFaker : Faker<AutomaticRule>
{
    public AutomaticRuleFaker(Guid userId)
    {
        RuleFor(a => a.ID, f => Guid.NewGuid())
            .RuleFor(a => a.UserID, f => userId)
            .FinishWith(
                (f, a) =>
                {
                    a.Conditions = new ConditionFaker(a.ID).Generate(f.Random.Int(1, 3));
                    a.Actions = new ActionFaker(a.ID).Generate(f.Random.Int(1, 3));
                }
            );
    }
}

public class ConditionFaker : Faker<RuleCondition>
{
    public ConditionFaker(Guid ruleId)
    {
        RuleFor(c => c.ID, f => Guid.NewGuid())
            .RuleFor(
                c => c.Field,
                f => f.PickRandom(AutomaticRuleConstants.TransactionFields.AllFields)
            )
            .RuleFor(
                c => c.Operator,
                f => f.PickRandom(new[] { "matches", "equals", "greater_than", "less_than" })
            )
            .RuleFor(c => c.Value, f => f.Lorem.Word())
            .RuleFor(c => c.RuleID, f => ruleId);
    }
}

public class ActionFaker : Faker<RuleAction>
{
    public ActionFaker(Guid ruleId)
    {
        RuleFor(a => a.ID, f => Guid.NewGuid())
            .RuleFor(
                a => a.Field,
                f => f.PickRandom(AutomaticRuleConstants.TransactionFields.AllFields)
            )
            .RuleFor(a => a.Operator, f => "set")
            .RuleFor(a => a.Value, f => f.Lorem.Word())
            .RuleFor(a => a.RuleID, f => ruleId);
    }
}
