namespace BudgetBoard.Service.Models;

public class AutomaticRuleConstants
{
    public struct TransactionFields
    {
        public const string Amount = "amount";
        public const string Date = "date";
        public const string Merchant = "merchant";
        public const string Category = "category";
    }

    public readonly struct ConditionalOperators
    {
        public static readonly string EqualsString = "equals";
        public static readonly string NotEquals = "notEquals";
        public static readonly string Contains = "contains";
        public static readonly string NotContains = "doesNotContain";
        public static readonly string StartsWith = "startsWith";
        public static readonly string EndsWith = "endsWith";
        public static readonly string GreaterThan = "greaterThan";
        public static readonly string LessThan = "lessThan";
        public static readonly string MatchesRegex = "matchesRegex";
        public static readonly string On = "on";
        public static readonly string Before = "before";
        public static readonly string After = "after";
        public static readonly string Is = "is";
        public static readonly string IsNot = "isNot";
        public static readonly IEnumerable<string> AllOperators =
        [
            EqualsString,
            NotEquals,
            Contains,
            NotContains,
            GreaterThan,
            LessThan,
            On,
            Before,
            After,
            Is,
            IsNot,
        ];
    }

    public readonly struct ActionOperators
    {
        public static readonly string Set = "set";
        public static readonly IEnumerable<string> AllOperators = [Set];
    }
}
