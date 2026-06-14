namespace BudgetBoard.Database.Models;

public static class AccountTypeClassification
{
    public const string Asset = "asset";
    public const string Liability = "liability";

    private static readonly HashSet<string> _allowedValues = new(
        [Asset, Liability],
        StringComparer.Ordinal
    );
    public static IReadOnlyCollection<string> AllowedValues => _allowedValues;

    public static bool IsValid(string source)
    {
        return _allowedValues.Contains(source);
    }
}
