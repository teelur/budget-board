namespace BudgetBoard.Database.Models;

public static class AccountSource
{
    public const string Manual = "Manual";
    public const string SimpleFIN = "SimpleFIN";
    public const string LunchFlow = "LunchFlow";

    public static readonly HashSet<string> AllowedValues = [Manual, SimpleFIN, LunchFlow];

    public static bool IsValid(string source)
    {
        return AllowedValues.Contains(source);
    }
}
