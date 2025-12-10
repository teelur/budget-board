/**
 * Checks if two strings are equal ignoring case.
 *
 * @param {string} string1 - The first string to compare.
 * @param {string} string2 - The second string to compare.
 * @returns {boolean} True if the strings are equal (ignoring case), false otherwise.
 */
export const areStringsEqual = (
  string1: string | null | undefined,
  string2: string | null | undefined
): boolean => string1?.toUpperCase() === string2?.toUpperCase();

/**
 * Calculates the progress percentage of an amount towards a total.
 *
 * @param {number} amount - The current amount achieved.
 * @param {number} total - The total goal amount.
 * @returns {number} The progress as a percentage, capped at 100 if exceeded.
 */
export const getProgress = (amount: number, total: number): number => {
  const percentage = (amount / total) * 100;
  return percentage > 100 ? 100 : percentage;
};

/**
 * Returns the full name of a month given its index.
 */
export const months = [...Array(12).keys()].map((key) =>
  new Date(0, key).toLocaleString("en", { month: "long" })
);

/**
 * Rounds a number away from zero. For positive numbers, it behaves like Math.round.
 * For negative numbers, it rounds away from zero instead of towards zero.
 *
 * @param {number} value - The number to round.
 */
export const roundAwayFromZero = (value: number): number =>
  value >= 0 ? Math.round(value) : Math.round(value * -1) * -1;
