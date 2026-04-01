import dayjs from "dayjs";

const DATE = 1;
const HOUR = 12;
const MINUTES = 0;
const SECONDS = 0;
const MILLISECONDS = 0;

// Mantine uses formatted date strings.
export const mantineDateFormat = "YYYY-MM-DD";
export const mantineDateTimeFormat = "YYYY-MM-DD HH:mm:ss";

/**
 * Returns the number of days elapsed since the specified date, as a string.
 *
 * The function calculates the difference in milliseconds between the current date/time
 * and the provided date, divides by the number of milliseconds in a day,
 * rounds the result, and returns it as a string.
 *
 * @param {Date} date - The date from which to calculate the elapsed days.
 * @returns {string} The number of days since the provided date, in string format.
 */
export const getDaysSinceDate = (date: Date): string => {
  const differenceInMs = new Date().getTime() - new Date(date).getTime();
  const differenceInDays = Math.round(differenceInMs / (1000 * 3600 * 24));
  return differenceInDays.toString();
};

/**
 * Creates and returns a new Date object, with the time set to specific default values.
 *
 * The function clones the provided Date object, then adjusts its hours, minutes,
 * seconds, and milliseconds to the predefined values: 12:00:00.000.
 *
 * @param {Date} date - The original date object to standardize.
 * @returns {Date} A new date object with standard time set.
 */
export const getStandardDate = (date: Date) =>
  dayjs(date)
    .hour(HOUR)
    .minute(MINUTES)
    .second(SECONDS)
    .millisecond(MILLISECONDS)
    .toDate();

/**
 * Initializes the current month by setting the date to 1, and time to 12:00:00.000.
 *
 * The function creates a new Date object for the current time, then standardizes
 * it to maintain consistency for month-based calculations.
 *
 * @returns {Date} A standardized Date object set to the first day of the current month.
 */
export const initCurrentMonth = (): Date => {
  const date = new Date();

  // We only really care about the month and year here, so we need to set
  // a consistent time for the rest.
  date.setDate(DATE);
  date.setHours(HOUR);
  date.setMinutes(MINUTES);
  date.setSeconds(SECONDS);
  date.setMilliseconds(MILLISECONDS);

  return date;
};

/**
 * Returns a standardized Date object from a specified number of months ago.
 *
 * The function calculates a Date object by subtracting the desired number of
 * months from the supplied date, or from the current month's standardized
 * date if no date is provided. It retains the same day and ensures consistent
 * time fields (12:00:00.000).
 *
 * @param {number} numberOfMonthsAgo - How many months in the past to go.
 * @param {Date} [date] - Optional starting date. If not provided, uses initCurrentMonth().
 * @returns {Date} A new Date object, shifted the specified number of months in the past.
 */
export const getDateFromMonthsAgo = (
  numberOfMonthsAgo: number,
  date?: Date
): Date => {
  const lastMonth = date ? new Date(date) : initCurrentMonth();

  lastMonth.setMonth(lastMonth.getMonth() - numberOfMonthsAgo);

  return lastMonth;
};

/**
 * Returns an array of unique years from an array of Date objects.
 *
 * The function maps the years from the provided dates array, then creates a new
 * Set to remove duplicates. Finally, it converts the Set back to an array.
 *
 * @param {Date[]} dates - Array of Date objects from which to extract years.
 * @returns {number[]} An array containing the unique years from the provided dates.
 */
export const getUniqueYears = (dates: Date[]): number[] =>
  Array.from(new Set(dates.map((date) => date.getFullYear())));

/**
 * Returns the total number of days in a specified month and year.
 *
 * @param {number} monthIndex - The month index (0-11).
 * @param {number} year - The full year (e.g., 2023).
 * @returns {number} The number of days in the given month of that year.
 */
export const getDaysInMonth = (monthIndex: number, year: number): number =>
  // Setting the date to 0 of the next month gives us the last day of the current month.
  new Date(year, monthIndex + 1, 0).getDate();

/**
 * Returns a localized month and year string for the provided date.
 *
 * @param {Date} date - The date to format.
 * @returns {string} The formatted month and year string.
 */
export const getMonthAndYearDateString = (date: Date, locale: string): string => {
  return date.toLocaleString(locale, { month: "long", year: "numeric" });
};

/**
 * Returns an array of unique dates from an array of Date objects.
 *
 * The function filters the provided dates array, comparing the index of each
 * date to the first index of that date. If the indexes match, the date is unique.
 *
 * @param {Date[]} dates - Array of Date objects from which to extract unique dates.
 * @returns {Date[]} An array containing the unique dates from the provided dates.
 */
export const getUniqueDates = (dates: Date[]): Date[] =>
  dates.filter(
    (date, index, array) =>
      array.findIndex((d) => d.getTime() === date.getTime()) === index
  );

/**
 * Determines whether two Date objects represent the same calendar day.
 *
 * Compares the year, month, and day components of both dates.
 *
 * @param date1 - The first date to compare.
 * @param date2 - The second date to compare.
 * @returns `true` if both dates are on the same year, month, and day; otherwise, `false`.
 */
export const areDatesEqual = (
  date1: Date | null,
  date2: Date | null
): boolean => {
  if (!date1 || !date2) {
    return false;
  }
  const formattedDate1 = new Date(date1);
  const formattedDate2 = new Date(date2);
  return (
    formattedDate1.getFullYear() === formattedDate2.getFullYear() &&
    formattedDate1.getMonth() === formattedDate2.getMonth() &&
    formattedDate1.getDate() === formattedDate2.getDate()
  );
};
