import { IBalanceResponse } from "~/models/balance";
import { getStandardDate } from "./datetime";

/**
 * Groups balances by account ID, sorted by dateTime in ascending order.
 * @param balances - Array of balance objects
 * @returns Map keyed by account ID with corresponding sorted balances
 */
export const mapAccountsToSortedBalances = (
  balances: IBalanceResponse[]
): Map<string, IBalanceResponse[]> => {
  const sortedBalances = balances.sort(
    (a, b) => new Date(a.dateTime).getTime() - new Date(b.dateTime).getTime()
  );

  const accountBalanceMap = Map.groupBy(
    sortedBalances,
    (balance: IBalanceResponse) => balance.accountID
  );

  return accountBalanceMap;
};

/**
 * Determines a standardized date from the given balances that is on or before
 * the specified start date. If no qualifying date exists, the function
 * returns the provided start date.
 *
 * @param balances - The list of account balance objects
 * @param startDate - The baseline date to compare against
 * @returns The standardized date that meets the criteria or the start date if none are found
 */
export const getInitialBalanceDate = (
  balances: IBalanceResponse[],
  startDate: Date
): Date =>
  // If an account is missing data for the specified startDate, we should try to find the closest date before the startDate.
  // This value will be undefined if there is no balance at or before the startDate. In that case we do not have any balances
  // before the start date and the balance will be zero until the first balance is found.
  getStandardDate(
    balances
      .slice()
      .reverse()
      .find((b) => new Date(b.dateTime).getTime() <= startDate.getTime())
      ?.dateTime ?? startDate
  );

/**
 * Filters a collection of balance entries by a specified date range.
 *
 * @param balances - A list of balance entries
 * @param startDate - The start date of the range
 * @param endDate - The end date of the range
 * @returns A new array of balance entries within the specified range
 */
export const filterBalancesByDateRange = (
  balances: IBalanceResponse[],
  startDate: Date,
  endDate: Date
): IBalanceResponse[] =>
  balances.filter(
    (balance) =>
      getStandardDate(balance.dateTime).getTime() >=
        getStandardDate(startDate).getTime() &&
      getStandardDate(balance.dateTime).getTime() <=
        getStandardDate(endDate).getTime()
  );
