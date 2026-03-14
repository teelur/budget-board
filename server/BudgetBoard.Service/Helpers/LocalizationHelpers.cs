using System.Globalization;
using System.Text.RegularExpressions;

namespace BudgetBoard.Service.Helpers;

/// <summary>
/// Helper class for localization-related utilities.
/// </summary>
public static partial class LocalizationHelpers
{
    /// <summary>
    /// A list of valid ISO 4217 currency codes.
    /// </summary>
    public static readonly List<string> CurrencyCodes =
    [
        .. CultureInfo
            .GetCultures(CultureTypes.SpecificCultures)
            .Select(culture => new RegionInfo(culture.Name))
            .Select(region => region.ISOCurrencySymbol)
            .Distinct()
            .OrderBy(currency => currency),
    ];

    /// <summary>
    /// Validates a date format string. Accepts "default" or formats like MM-DD-YYYY, DD/MM/YYYY, YYYY.MM.DD
    /// with any single non-alphanumeric separator character. Ensures MM, DD, and YYYY each appear exactly once.
    /// </summary>
    /// <param name="dateFormat">The date format string to validate.</param>
    /// <returns>True if the date format is valid, false otherwise.</returns>
    public static bool IsValidDateFormat(string dateFormat)
    {
        if (dateFormat == "default")
            return true;

        // Match pattern: (MM|DD|YYYY) + single non-alphanumeric separator + (MM|DD|YYYY) + separator + (MM|DD|YYYY)
        var match = DateFormatRegex().Match(dateFormat);

        if (!match.Success)
            return false;

        var parts = new[] { match.Groups[1].Value, match.Groups[3].Value, match.Groups[5].Value };

        var validParts = new HashSet<string> { "MM", "DD", "YYYY" };
        return parts.All(p => validParts.Contains(p)) && parts.Distinct().Count() == 3;
    }

    [GeneratedRegex(@"^(MM|DD|YYYY)([^a-zA-Z0-9])(MM|DD|YYYY)([^a-zA-Z0-9])(MM|DD|YYYY)$")]
    private static partial Regex DateFormatRegex();
}
