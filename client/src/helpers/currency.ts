/**
 * Converts a number to a formatted USD currency string.
 *
 * This function formats the provided number as a USD currency string using the Intl.NumberFormat API.
 * It allows including cents in the formatted output if specified.
 * Adding 0 to the number ensures that negative zero (-0) is avoided.
 *
 * @param {number} number - The numeric value to convert.
 * @param {boolean} [shouldIncludeCents] - Optional boolean flag to include cents in the output.
 * @returns {string} The number formatted as USD currency.
 */
export const convertNumberToCurrency = (
  number: number,
  shouldIncludeCents: boolean,
  currency: string
) => {
  // Adding 0 to avoid -0 for the output.
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency,
    maximumFractionDigits: shouldIncludeCents ? 2 : 0,
    minimumFractionDigits: shouldIncludeCents ? 2 : 0,
  }).format(number + 0);
};

/**
 * Returns the currency symbol for a given currency code.
 *
 * @param currency - The ISO 4217 currency code (e.g., "USD", "EUR").
 * @returns The corresponding currency symbol if defined, otherwise returns the currency code itself.
 *
 * @example
 * getCurrencySymbol("USD"); // Returns "$"
 * getCurrencySymbol("EUR"); // Returns "€"
 * getCurrencySymbol("INR"); // Returns "INR"
 */
export const getCurrencySymbol = (currency?: string): string => {
  switch (currency) {
    case "USD":
      return "$";
    case "EUR":
      return "€";
    case "GBP":
      return "£";
    case "JPY":
      return "¥";
    case "AUD":
      return "A$";
    case "CAD":
      return "C$";
    case "CHF":
      return "CHF";
    case "CNY":
      return "¥"; // Chinese Yuan uses the same symbol as JPY
    case null:
    case undefined:
      return ""; // Return an empty string if no currency is provided
    default:
      return currency; // Return the currency code if no symbol is defined
  }
};
