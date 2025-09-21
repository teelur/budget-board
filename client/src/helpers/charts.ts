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
import { IAccountResponse, liabilityAccountTypes } from "~/models/account";

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
  transactions: ITransaction[]
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
          : getDaysInMonth(month.getMonth(), month.getFullYear())
      );

    rollingTotalTransactionsForMonth.forEach(
      (rollingTotalTransaction: RollingTotalSpendingPerDay) => {
        const chartDay = spendingTrendsChartData.find(
          (data) => data.day === rollingTotalTransaction.day
        );

        // On the very first loop, we need to create the data point.
        if (chartDay == null) {
          const newChartDay: any = {
            day: rollingTotalTransaction.day,
            [getMonthAndYearDateString(month)]: rollingTotalTransaction.amount,
          };
          spendingTrendsChartData.push(newChartDay);
        } else {
          chartDay[getMonthAndYearDateString(month)] =
            rollingTotalTransaction.amount;
        }
      }
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
  months: Date[]
): { name: string; color: string }[] =>
  months.map((month, i) => ({
    name: getMonthAndYearDateString(month),
    color: chartColors[i % chartColors.length] ?? "gray.6",
  }));

interface BalanceChartData {
  date: Date;
  dateString: string;
  [key: string]: number | Date | string;
}

/**
 * Builds data for an account balance chart based on provided balances and date range.
 *
 * @param balances An array of IBalance objects representing account balances.
 * @param startDate The start date for the chart data.
 * @param endDate The end date for the chart data.
 * @param invertData Optional. If true, inverts the balance data (e.g., for representing expenses as negative values). Defaults to false.
 * @returns An array of objects, where each object represents a date and the corresponding balance for each account on that date.
 */
export const buildAccountBalanceChartData = (
  balances: IBalance[],
  invertData = false
) => {
  const sortedBalances = balances.sort(
    (a, b) => new Date(a.dateTime).getTime() - new Date(b.dateTime).getTime()
  );

  const accountIdToSortedBalancesMap = Map.groupBy(
    sortedBalances,
    (balance: IBalance) => balance.accountID
  );

  // When multiple accounts are selected, some dates might not be represented on all accounts.
  // We need to aggregate all dates that have an associated balance for at least one account.
  const distinctSortedBalanceDates: Date[] = getDistinctDates(
    getSortedBalanceDates(balances)
  );

  const chartData: BalanceChartData[] = [];

  distinctSortedBalanceDates.forEach((date: Date, index: number) => {
    // TODO: This is a hack to get around charts not liking non-string keys.
    // It works with shadcn, so need to figure out how to make it work here.
    const dateString = date.toLocaleDateString("en-US", {
      month: "numeric",
      day: "numeric",
    });

    const chartDataPoint: BalanceChartData = { date, dateString };

    accountIdToSortedBalancesMap.forEach((accountBalances, accountId) => {
      const balancesForDate = accountBalances.filter(
        (balance) =>
          getStandardDate(new Date(balance.dateTime)).getTime() ===
          getStandardDate(date).getTime()
      );

      if (balancesForDate.length > 0) {
        chartDataPoint[accountId] =
          (balancesForDate.reduce((acc, b) => acc + b.amount, 0) /
            balancesForDate.length) *
          (invertData ? -1 : 1);
      } else if (index > 0) {
        chartDataPoint[accountId] = chartData[index - 1]![accountId]!;
      } else {
        chartDataPoint[accountId] = 0;
      }
    });

    chartData.push(chartDataPoint);
  });

  return chartData;
};

/**
 * Builds the series for the account balance chart.
 *
 * @param accounts An array of IAccount objects representing accounts.
 * @returns An array of objects containing the name of the account and the color to use.
 */
export const buildAccountBalanceChartSeries = (accounts: IAccountResponse[]) =>
  accounts.map((account: IAccountResponse) => {
    return {
      name: account.id,
      label: account.name,
      color:
        chartColors[accounts.indexOf(account) % chartColors.length] ?? "gray.6",
    };
  });

/**
 * Represents a data point for net worth, including assets, liabilities, and net value on a specific date.
 */
export interface NetWorthChartData {
  date: Date;
  dateString: string;
  assets: number;
  liabilities: number;
  net: number;
}

/**
 * Builds data for a net worth chart based on provided balances and accounts.
 *
 * @param balances An array of IBalance objects representing account balances.
 * @param accounts An array of IAccount objects representing accounts.
 * @returns An array of objects, where each object represents a date and the corresponding net worth data.
 */
export const BuildNetWorthChartData = (
  balances: IBalance[],
  accounts: IAccountResponse[]
): NetWorthChartData[] => {
  // Use the account balance chart data to build the net worth chart data.
  const accountChartData = buildAccountBalanceChartData(balances);

  const chartData: NetWorthChartData[] = [];
  accountChartData.forEach((dataPoint) => {
    const chartDataPoint: NetWorthChartData = {
      date: dataPoint.date,
      dateString: dataPoint.dateString,
      assets: 0,
      liabilities: 0,
      net: 0,
    };

    accounts.forEach((account) => {
      const chartIndex = liabilityAccountTypes.includes(account.type)
        ? "liabilities"
        : "assets";

      chartDataPoint[chartIndex] += (dataPoint[account.id] as number) ?? 0;
    });

    chartDataPoint.net = chartDataPoint.assets + chartDataPoint.liabilities;

    chartData.push(chartDataPoint);
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
  invertData: boolean
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
          0
        ) * (invertData ? -1 : 1),
    });
  });
  return monthlySpendingChartData;
};
