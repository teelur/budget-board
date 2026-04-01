import { ITransaction } from "~/models/transaction";
import {
  getRollingTotalSpendingForMonth,
  getTransactionsForMonth,
  RollingTotalSpendingPerDay,
} from "./transactions";
import {
  getDaysInMonth,
  getMonthAndYearDateString,
  getStandardDate,
  getUniqueDates as getDistinctDates,
} from "./datetime";
import { IBalance } from "~/models/balance";
import { getSortedBalanceDates } from "./balances";
import { IAccount, liabilityAccountTypes } from "~/models/account";
import { areStringsEqual } from "./utils";
import { getFormattedCategoryValue } from "./category";
import { ICategory } from "~/models/category";

export const chartColors = [
  "indigo.6",
  "teal.6",
  "orange.6",
  "red.6",
  "yellow.6",
  "lime.6",
  "grape.6",
  "pink.6",
];

/**
 * Builds a dataset showing the cumulative spending trend for the given months.
 *
 * @param months - An array of Date objects representing each month to include.
 * @param transactions - A collection of transaction objects containing spending information.
 * @returns An array of objects containing day-by-day spending data across months.
 */
export const buildTransactionChartData = (
  months: Date[],
  transactions: ITransaction[],
  formatDateString: (date: Date) => string,
): any[] => {
  const spendingTrendsChartData: any[] = [];
  months.forEach((month) => {
    const transactionsForMonth = getTransactionsForMonth(transactions, month);

    // If it is the current month, we only want to show the data up to today's date.
    const today = new Date();
    const isThisMonth =
      month.getMonth() === today.getMonth() &&
      month.getFullYear() === today.getFullYear();

    const rollingTotalTransactionsForMonth: RollingTotalSpendingPerDay[] =
      getRollingTotalSpendingForMonth(
        transactionsForMonth,
        isThisMonth
          ? today.getDate()
          : getDaysInMonth(month.getMonth(), month.getFullYear()),
      );

    rollingTotalTransactionsForMonth.forEach(
      (rollingTotalTransaction: RollingTotalSpendingPerDay) => {
        const chartDay = spendingTrendsChartData.find(
          (data) => data.day === rollingTotalTransaction.day,
        );

        // On the very first loop, we need to create the data point.
        if (chartDay == null) {
          const newChartDay: any = {
            day: rollingTotalTransaction.day,
            [formatDateString(month)]: rollingTotalTransaction.amount,
          };
          spendingTrendsChartData.push(newChartDay);
        } else {
          chartDay[formatDateString(month)] = rollingTotalTransaction.amount;
        }
      },
    );
  });
  return spendingTrendsChartData;
};

/**
 * Builds the series for the transaction chart.
 *
 * @param months - An array of Date objects representing each month to include.
 * @returns An array of objects containing the name of the month and the color to use.
 */
export const buildTransactionChartSeries = (
  months: Date[],
  formatDateString: (date: Date) => string,
): { name: string; color: string }[] =>
  months.map((month, i) => ({
    name: formatDateString(month),
    color: chartColors[i % chartColors.length] ?? "gray.6",
  }));

/**
 * Builds chart data for spending categories based on a list of transactions and categories.
 *
 * Iterates through each transaction, determines its formatted category name,
 * and aggregates the transaction amounts by category. The result is an array
 * of objects, each representing a category and the total amount spent in that category.
 *
 * @param transactions - An array of transaction objects to be aggregated.
 * @param categories - An array of category objects used to format and match transaction categories.
 * @returns An array of objects, each containing a `name` (category) and `value` (total amount spent).
 */
export const BuildSpendingCategoryChartData = (
  transactions: ITransaction[],
  categories: ICategory[],
) => {
  const chartData: any[] = [];

  const filteredTransactions = transactions.filter(
    (transaction) =>
      !areStringsEqual(transaction.category ?? "", "Income") &&
      !areStringsEqual(transaction.category ?? "", "Hide from Budgets"),
  );

  filteredTransactions.forEach((transaction) => {
    const formattedTransactionCategory = getFormattedCategoryValue(
      transaction.category ?? "",
      categories,
    );
    const chartDataPoint = chartData.find((data) =>
      areStringsEqual(data.name, formattedTransactionCategory),
    );

    if (chartDataPoint == null) {
      chartData.push({
        name: formattedTransactionCategory,
        value: transaction.amount * -1,
        color: chartColors[chartData.length % chartColors.length] ?? "gray.6",
      });
    } else {
      chartDataPoint.value += transaction.amount * -1;
    }
  });

  return chartData;
};

interface MonthlySpendingData {
  month: string;
  total: number;
}

export const buildMonthlySpendingChartData = (
  months: Date[],
  transactions: ITransaction[],
  invertData: boolean,
): MonthlySpendingData[] => {
  const monthlySpendingChartData: MonthlySpendingData[] = [];
  months.forEach((month) => {
    const transactionsForMonth = getTransactionsForMonth(transactions, month);

    monthlySpendingChartData.push({
      month: month.toLocaleString("default", {
        month: "numeric",
        year: "2-digit",
      }),
      total:
        transactionsForMonth.reduce(
          (acc, transaction) => acc + transaction.amount,
          0,
        ) * (invertData ? -1 : 1),
    });
  });
  return monthlySpendingChartData;
};
