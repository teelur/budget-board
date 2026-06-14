namespace BudgetBoard.Database.Models;

public static class AccountSource
{
    public const string Manual = "Manual";
    public const string SimpleFIN = "SimpleFIN";
    public const string LunchFlow = "LunchFlow";

    private static readonly HashSet<string> _allowedValues = new(
        [Manual, SimpleFIN, LunchFlow],
        StringComparer.Ordinal
    );
    public static IReadOnlyCollection<string> AllowedValues => _allowedValues;

    public static bool IsValid(string source)
    {
        return _allowedValues.Contains(source);
    }
}
