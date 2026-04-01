import { IAccountResponse, liabilityAccountTypes } from "~/models/account";
import { IBalanceResponse } from "~/models/balance";
import { buildValueChartData } from "../../ValueChart/helpers/valueChart";

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
  balances: IBalanceResponse[],
  accounts: IAccountResponse[],
  formatDateString: (date: Date) => string,
): NetWorthChartData[] => {
  // Use the account balance chart data to build the net worth chart data.
  const valuesWithParentId = balances.map((balance) => ({
    ...balance,
    parentId: balance.accountID,
  }));
  const accountChartData = buildValueChartData(
    valuesWithParentId,
    formatDateString,
  );

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
