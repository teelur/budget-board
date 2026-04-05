import { hiddenTransactionCategory, ITransaction } from "~/models/transaction";
import {
  getRollingTotalSpendingForMonth,
  getTransactionsForMonth,
  RollingTotalSpendingPerDay,
} from "./transactions";
import { getDaysInMonth } from "./datetime";
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
export const buildSpendingCategoryChartData = (
  transactions: ITransaction[],
  categories: ICategory[],
) => {
  const filteredTransactions = transactions.filter(
    (transaction) =>
      !areStringsEqual(transaction.category ?? "", "Income") &&
      !areStringsEqual(transaction.category ?? "", hiddenTransactionCategory),
  );

  const totalsMap = new Map<string, number>();

  filteredTransactions.forEach((transaction) => {
    const formattedTransactionCategory = getFormattedCategoryValue(
      transaction.category ?? "",
      categories,
    );
    totalsMap.set(
      formattedTransactionCategory,
      (totalsMap.get(formattedTransactionCategory) ?? 0) +
        transaction.amount * -1,
    );
  });

  return Array.from(totalsMap.entries()).map(([name, value], i) => ({
    name,
    value,
    color: chartColors[i % chartColors.length] ?? "gray.6",
  }));
};

/**
 * Builds subcategory-level chart data for the outer ring of a two-ring pie chart.
 * Entries are ordered to align with the inner ring (grouped by parent).
 * Color shades are derived from the parent's color family.
 */
export const buildSpendingSubcategoryChartData = (
  transactions: ITransaction[],
  categories: ICategory[],
  innerChartData: { name: string; color: string }[],
): any[] => {
  const filteredTransactions = transactions.filter(
    (transaction) =>
      !areStringsEqual(transaction.category ?? "", "Income") &&
      !areStringsEqual(transaction.category ?? "", hiddenTransactionCategory),
  );

  const subMap = new Map<
    string,
    { name: string; value: number; parent: string }
  >();

  filteredTransactions.forEach((transaction) => {
    const parentName = getFormattedCategoryValue(
      transaction.category ?? "",
      categories,
    );
    const subName = transaction.subcategory
      ? getFormattedCategoryValue(transaction.subcategory, categories)
      : parentName;
    const key = `${parentName}::${subName}`;
    const existing = subMap.get(key);
    if (existing) {
      existing.value += transaction.amount * -1;
    } else {
      subMap.set(key, {
        name: subName,
        value: transaction.amount * -1,
        parent: parentName,
      });
    }
  });

  const shadeSteps = [4, 7, 3, 8, 2, 9, 5];
  const result: any[] = [];

  innerChartData.forEach((parent) => {
    const colorFamily = parent.color.split(".")[0] ?? "gray";
    const subs = [...subMap.values()].filter((s) => s.parent === parent.name);
    subs.forEach((sub, i) => {
      const shade =
        subs.length === 1 ? 6 : (shadeSteps[i % shadeSteps.length] ?? 6);
      result.push({
        name: sub.name,
        value: sub.value,
        color: `${colorFamily}.${shade}`,
        parent: sub.parent,
      });
    });
  });

  return result;
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
