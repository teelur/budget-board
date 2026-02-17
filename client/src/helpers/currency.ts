export enum SignDisplay {
  Auto = "auto",
  Always = "always",
  Never = "never",
  ExceptZero = "exceptZero",
}

/**
 * Format a numeric value as a localized currency string.
 *
 * This wrapper around `Intl.NumberFormat` formats the given numeric value using the
 * provided ISO 4217 currency code. It ensures that negative zero (`-0`) is normalized
 * to `0` in the output by adding `0` to the input number before formatting.
 *
 * Notes:
 * - `shouldIncludeCents` controls whether two fraction digits are shown (e.g. cents).
 * - The function uses the runtime/default locale (by passing `undefined`) so the
 *   formatted output respects the user's locale settings.
 *
 * @param number - The numeric value to format.
 * @param shouldIncludeCents - When `true`, output includes two fraction digits; when
 *   `false`, output shows no fraction digits.
 * @param currency - The ISO 4217 currency code to use (e.g. `"USD"`, `"EUR"`).
 * @returns The formatted currency string for the current locale (for example `"$1,234.56"`).
 */
export const convertNumberToCurrency = (
  number: number,
  shouldIncludeCents: boolean,
  currency: string,
  signDisplay: SignDisplay,
  locale: string,
): string => {
  // Adding 0 to avoid -0 for the output.
  return new Intl.NumberFormat(locale, {
    style: "currency",
    currency,
    maximumFractionDigits: shouldIncludeCents ? 2 : 0,
    minimumFractionDigits: shouldIncludeCents ? 2 : 0,
    signDisplay: signDisplay ?? SignDisplay.Auto,
  }).format(number + 0);
};

/**
 * Return the display symbol for a currency code, using the runtime's `Intl` support.
 *
 * The function attempts to extract the localized currency symbol (for example `$`, `€`)
 * by formatting a value with `Intl.NumberFormat.formatToParts` and reading the `currency`
 * part. If the runtime does not support the provided currency code, or an error
 * occurs, the function falls back to returning the upper-cased currency code.
 *
 * Behavior:
 * - If `currency` is `undefined` or an empty string, returns an empty string.
 * - If the symbol can be extracted from `Intl`, returns that symbol.
 * - On error or unsupported currency, returns the upper-cased currency code (e.g. `"INR"`).
 *
 * @param currency - Optional ISO 4217 currency code (e.g. `"USD"`, `"EUR"`).
 * @returns The localized currency symbol, or the upper-cased currency code, or an empty string
 *   when no currency was provided.
 * @example
 * getCurrencySymbol("USD"); // => "$"
 * getCurrencySymbol("EUR"); // => "€"
 * getCurrencySymbol("INR"); // => "INR" (if runtime doesn't provide a specific symbol)
 */
export const getCurrencySymbol = (currency?: string): string => {
  // Return empty string for no currency
  if (currency == null || currency === "") {
    return "";
  }

  // Try to use Intl.NumberFormat.formatToParts to extract the currency symbol.
  // This works for any valid ISO 4217 currency code supported by the runtime.
  try {
    const parts = new Intl.NumberFormat(undefined, {
      style: "currency",
      currency,
      currencyDisplay: "symbol",
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).formatToParts(1);

    const symbolPart = parts.find((p) => p.type === "currency");
    if (symbolPart && symbolPart.value !== undefined) {
      return symbolPart.value;
    }
  } catch {
    // Suppress logging to avoid unexpected console statements in restricted environments
    // and fall back to returning the currency code in uppercase.
    return currency.toUpperCase();
  }

  // Fallback to returning the currency code in uppercase if symbol extraction fails.
  return currency.toUpperCase();
};
