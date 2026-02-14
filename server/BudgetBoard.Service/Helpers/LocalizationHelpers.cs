using System.Globalization;

namespace BudgetBoard.Service.Helpers;

/// <summary>
/// Helper class for localization-related utilities.
/// </summary>
public static class LocalizationHelpers
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

    public static readonly List<string> DateFormats =
    [
        "default", // Use system default
        "MM/DD/YYYY", // US format
        "DD/MM/YYYY", // European format
        "YYYY/MM/DD", // ISO format
    ];
}
