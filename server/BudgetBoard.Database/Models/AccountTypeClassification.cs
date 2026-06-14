namespace BudgetBoard.Database.Models;

public static class AccountTypeClassification
{
    public const string Asset = "Asset";
    public const string Liability = "Liability";

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
