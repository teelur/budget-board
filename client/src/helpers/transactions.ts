import { Sorts } from "~/app/Authorized/PageContent/Transactions/TransactionsHeader/SortMenu/SortMenuHelpers";
import { SortDirection } from "~/components/SortButton";
import {
  Filters,
  hiddenTransactionCategory,
  ITransaction,
  uncategorizedTransactionCategory,
} from "~/models/transaction";
import { areStringsEqual } from "./utils";
import { getIsParentCategory } from "./category";
import dayjs from "~/shared/dayjs";
import { ICategory } from "~/models/category";

/**
 * Sorts an array of transactions using the specified field and direction.
 *
 * This function modifies the provided transactions array in place based on a switch statement
 * handling different sort criteria: Date, Merchant, Category, or Amount.
 *
 * @param {ITransaction[]} transactions - Array of transaction objects.
 * @param {Sorts} sortValue - The transaction field on which to sort.
 * @param {SortDirection} sortDirection - Indicates ascending or descending sort.
 * @returns {ITransaction[]} The sorted array of transaction objects.
 */
export const sortTransactions = (
  transactions: ITransaction[],
  sortValue: Sorts,
  sortDirection: SortDirection,
): ITransaction[] => {
  switch (sortValue) {
    case Sorts.Date:
      return sortDirection === SortDirection.Decending
        ? transactions.sort(
            (a, b) => dayjs(b.date).valueOf() - dayjs(a.date).valueOf(),
          )
        : transactions.sort(
            (a, b) => dayjs(a.date).valueOf() - dayjs(b.date).valueOf(),
          );
    case Sorts.Merchant:
      return sortDirection === SortDirection.Decending
        ? transactions.sort((a, b) =>
            (b.merchantName ?? "")
              .toLocaleLowerCase()
              .localeCompare((a.merchantName ?? "").toLocaleLowerCase()),
          )
        : transactions.sort((a, b) =>
            (a.merchantName ?? "")
              .toLocaleLowerCase()
              .localeCompare((b.merchantName ?? "").toLocaleLowerCase()),
          );
    case Sorts.Category:
      return sortDirection === SortDirection.Decending
        ? transactions.sort((a, b) =>
            (b.subcategory === null || b.subcategory === ""
              ? (b.category ?? uncategorizedTransactionCategory)
              : (b.subcategory ?? uncategorizedTransactionCategory)
            )
              .toLocaleLowerCase()
              .localeCompare(
                (a.subcategory === null || a.subcategory === ""
                  ? (a.category ?? uncategorizedTransactionCategory)
                  : (a.subcategory ?? uncategorizedTransactionCategory)
                ).toLocaleLowerCase(),
              ),
          )
        : transactions.sort((a, b) =>
            (a.subcategory === null || a.subcategory === ""
              ? (a.category ?? uncategorizedTransactionCategory)
              : (a.subcategory ?? uncategorizedTransactionCategory)
            )
              .toLocaleLowerCase()
              .localeCompare(
                (b.subcategory === null || b.subcategory === ""
                  ? (b.category ?? uncategorizedTransactionCategory)
                  : (b.subcategory ?? uncategorizedTransactionCategory)
                ).toLocaleLowerCase(),
              ),
          );
    case Sorts.Amount:
      return sortDirection === SortDirection.Decending
        ? transactions.sort((a, b) => (a.amount < b.amount ? 1 : -1))
        : transactions.sort((a, b) => (a.amount > b.amount ? 1 : -1));
    default:
      return transactions;
  }
};

/**
 * Retrieves the category or subcategory for a transaction.
 *
 * This function returns the subcategory if it exists, otherwise it returns the category.
 *
 * @param {string} category - The transaction's category.
 * @param {string} subcategory - The transaction's subcategory.
 * @returns {string} The transaction's category or subcategory.
 */
export const getTransactionCategory = (
  category: string,
  subcategory: string,
): string => (subcategory && subcategory.length > 0 ? subcategory : category);

/**
 * Filters out transactions that have not been deleted.
 *
 * The function filters the transactions array, excluding those with a null 'deleted' field.
 *
 * @param {ITransaction[]} transactions - Array of transaction objects.
 * @returns {ITransaction[]} The filtered array of transactions.
 */
export const getVisibleTransactions = (
  transactions: ITransaction[],
): ITransaction[] =>
  transactions.filter((t: ITransaction) => t.deleted === null);

/**
 * Filters transactions based on accounts, categories, date range, merchant name, and amount range.
 *
 * The function starts by excluding deleted transactions. It then applies:
 * - Account filters (if any are selected).
 * - Category filters (checks if the category is a parent or subcategory).
 * - Date range filters (if dates are specified).
 * - Merchant name filter (if specified).
 * - Amount range filters (if specified).
 *
 * @param {ITransaction[]} transactions - Array of transaction objects.
 * @param {Filters} filters - Object containing account, category, date range, merchant name, and amount range filters.
 * @param {ICategory[]} transactionCategories - Array of all categories, used to check for parent categories.
 * @returns {ITransaction[]} The filtered array of transactions.
 */
export const getFilteredTransactions = (
  transactions: ITransaction[],
  filters: Filters,
  transactionCategories: ICategory[],
): ITransaction[] => {
  // We don't want to include deleted transactions.
  let filteredTransactions = getVisibleTransactions(transactions);
  if (filters.accounts.length > 0) {
    filteredTransactions = filteredTransactions.filter((t) =>
      filters.accounts.some((f) => areStringsEqual(f, t.accountID)),
    );
  }
  if (filters.category && filters.category.length > 0) {
    // Uncategorized is a special case since it is not in the categories list.
    const filterCategory = areStringsEqual(
      filters.category,
      uncategorizedTransactionCategory,
    )
      ? ""
      : filters.category;
    filteredTransactions = filteredTransactions.filter((t) =>
      getIsParentCategory(filters.category, transactionCategories)
        ? areStringsEqual(t.category ?? "", filterCategory)
        : areStringsEqual(t.subcategory ?? "", filterCategory),
    );
  }
  if (filters.dateRange?.at(0)) {
    filteredTransactions = filteredTransactions.filter((t) =>
      dayjs(t.date).isSameOrAfter(dayjs(filters.dateRange.at(0)!), "day"),
    );
  }
  if (filters.dateRange?.at(1)) {
    filteredTransactions = filteredTransactions.filter((t) =>
      dayjs(t.date).isSameOrBefore(dayjs(filters.dateRange.at(1)!), "day"),
    );
  }
  if (filters.merchantName && filters.merchantName.length > 0) {
    filteredTransactions = filteredTransactions.filter((t) =>
      (t.merchantName ?? "")
        .toLowerCase()
        .includes(filters.merchantName.toLowerCase()),
    );
  }
  if (filters.amountRange[0] !== null) {
    filteredTransactions = filteredTransactions.filter(
      (t) => t.amount >= filters.amountRange[0]!,
    );
  }
  if (filters.amountRange[1] !== null) {
    filteredTransactions = filteredTransactions.filter(
      (t) => t.amount <= filters.amountRange[1]!,
    );
  }
  return filteredTransactions;
};

/**
 * Retrieves an array of transactions that are marked as deleted.
 *
 * The function examines each transaction's 'deleted' property and filters
 * those with a non-null 'deleted' field, indicating they were removed by the user.
 *
 * @param {ITransaction[]} transactions - Array of all transaction objects.
 * @returns {ITransaction[]} An array containing the deleted transactions.
 */
export const getDeletedTransactions = (
  transactions: ITransaction[],
): ITransaction[] =>
  transactions.filter((t: ITransaction) => t.deleted !== null);

/**
 * Builds a map from each month's timestamp to the sum of transaction amounts for that month.
 *
 * The function iterates over the provided months array, calculates the total
 * transaction amounts within each month, and stores them in a map keyed by
 * the month’s timestamp (from getTime()).
 *
 * @param {Date[]} months - Array of Date objects representing the months to evaluate.
 * @param {ITransaction[]} transactions - Array of transaction objects.
 * @returns {Map<number, number>} A map where each key is the timestamp of the month, and each value is the total transaction amount for that month.
 */
export const buildTimeToMonthlyTotalsMap = (
  months: Date[],
  transactions: ITransaction[],
): Map<number, number> => {
  const map = new Map<number, number>();
  months.forEach((month: Date) => {
    const total = transactions
      .filter((t) => dayjs(t.date).isSame(month, "month"))
      .reduce((n, { amount }) => n + amount, 0);
    map.set(month.getTime(), total);
  });
  return map;
};

/**
 * Filters out transactions with the hiddenTransactionCategory.
 *
 * The function filters the transactions array, excluding those with the hiddenTransactionCategory.
 *
 * @param {ITransaction[]} transactions - Array of transaction objects.
 * @returns {ITransaction[]} The filtered array of transactions.
 */
export const filterHiddenTransactions = (transactions: ITransaction[]) =>
  getVisibleTransactions(transactions).filter(
    (t) => !areStringsEqual(t.category ?? "", hiddenTransactionCategory),
  );

/**
 * Retrieves transactions for a specific month and year based on the provided date.
 *
 * The function filters the given transaction array, matching only those
 * whose month and year coincide with the month and year of the specified date.
 *
 * @param {ITransaction[]} transactionData - The array of transactions to filter.
 * @param {Date} date - The reference date used to match the month and year.
 * @returns {ITransaction[]} An array of transactions occurring in the specified month and year.
 */
export const getTransactionsForMonth = (
  transactionData: ITransaction[],
  date: Date,
): ITransaction[] =>
  transactionData.filter((t: ITransaction) =>
    dayjs(t.date).isSame(date, "month"),
  );

export interface RollingTotalSpendingPerDay {
  day: number;
  amount: number;
}

/**
 * Calculates daily cumulative spending for a specified month, excluding "Income."
 * Transactions are sorted by date, their amounts are subtracted from a running total,
 * and missing days carry over the previous day's total.
 *
 * @param {ITransaction[]} transactionsForMonth - The month's transactions
 * @param {number} endDate - The last day of the month
 * @returns {RollingTotalSpendingPerDay[]} Array of day-and-amount pairs
 */
export const getRollingTotalSpendingForMonth = (
  transactionsForMonth: ITransaction[],
  endDate: number,
): RollingTotalSpendingPerDay[] => {
  const rollingTotalSpendingPerDay: RollingTotalSpendingPerDay[] = [];

  const sortedSpending = transactionsForMonth
    .filter((t) => !areStringsEqual(t.category ?? "", "Income"))
    .sort((a, b) => dayjs(a.date).valueOf() - dayjs(b.date).valueOf());

  let total = 0;
  const summedTransactionsPerMonth = sortedSpending.reduce(
    (result: RollingTotalSpendingPerDay[], transaction: ITransaction) => {
      const foundDay = result.find(
        (t) => t.day === dayjs(transaction.date).date(),
      );

      // Transactions are negative and we want spending to be positive,
      // so instead of multiplying by a negative, just subtract from total.
      total -= transaction.amount;

      if (foundDay == null) {
        const newDay: RollingTotalSpendingPerDay = {
          day: dayjs(transaction.date).date(),
          amount: total,
        };
        result.push(newDay);
      } else {
        foundDay.amount = total;
      }
      return result;
    },
    [],
  );

  // If it is the current month, we need to continue the rolling total
  for (let dayItr = 1; dayItr <= endDate; dayItr++) {
    const amount = summedTransactionsPerMonth.find((t) => t.day === dayItr);
    const dayAmount: RollingTotalSpendingPerDay = {
      day: dayItr,
      amount: 0,
    };
    if (!amount) {
      if (dayItr > 1) {
        // If there are no transactions for a day, we should just roll over the previous day.
        dayAmount.amount = rollingTotalSpendingPerDay[dayItr - 2]!.amount;
      }
      rollingTotalSpendingPerDay.push(dayAmount);
    } else {
      rollingTotalSpendingPerDay.push(amount);
    }
  }

  return rollingTotalSpendingPerDay;
};

/**
 * Retrieves transactions for a specific category.
 *
 * The function filters the given transaction array, matching only those
 * whose category or subcategory matches the provided categoryValue.
 *
 * @param {ITransaction[]} transactions - Array of transaction objects.
 * @param {string} categoryValue - The category to match.
 * @returns {ITransaction[]} An array of transactions for the specified category.
 */
export const getTransactionsByCategory = (
  transactions: ITransaction[],
  categoryValue: string,
) =>
  transactions.filter((t: ITransaction) =>
    areStringsEqual(t.category ?? "", categoryValue),
  );

/**
 * Builds a map of total transaction amounts keyed by category or subcategory.
 *
 * @param {ITransaction[]} transactions - Array of transaction objects
 * @returns {Map<string, number>} A map with categories/subcategories as keys and summed transaction amounts as values
 */
export const buildCategoryToTransactionsTotalMap = (
  transactions: ITransaction[],
): Map<string, number> => {
  const categoryToTransactionsTotalMap = new Map<string, number>();
  transactions.forEach((transaction) => {
    const currentTotalCategory =
      categoryToTransactionsTotalMap.get(
        (transaction.category ?? "").toLocaleLowerCase(),
      ) ?? 0;

    categoryToTransactionsTotalMap.set(
      (transaction.category ?? "").toLocaleLowerCase(),
      currentTotalCategory + transaction.amount,
    );

    if (
      transaction.subcategory !== null &&
      transaction.subcategory !== "" &&
      transaction.subcategory.toLocaleLowerCase() !==
        (transaction.category ?? "").toLocaleLowerCase()
    ) {
      const currentTotalSubCategory =
        categoryToTransactionsTotalMap.get(
          transaction.subcategory.toLocaleLowerCase(),
        ) ?? 0;
      categoryToTransactionsTotalMap.set(
        transaction.subcategory.toLocaleLowerCase(),
        currentTotalSubCategory + transaction.amount,
      );
    }
  });
  return categoryToTransactionsTotalMap;
};

/**
 * Calculates the sum of transaction amounts.
 *
 * @param {ITransaction[]} transactionData - Array of transaction objects
 * @returns {number} The total sum of all amounts
 */
export const sumTransactionAmounts = (
  transactionData: ITransaction[],
): number => {
  return transactionData.reduce((n, { amount }) => n + amount, 0);
};
