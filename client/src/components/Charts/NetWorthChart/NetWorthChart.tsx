import {
  NetWorthChart as BBNetWorthChart,
  type NetWorthChartData,
} from "@teelur/budget-board-ui";
import { filterBalancesByDateRange } from "~/helpers/balances";
import { SignDisplay } from "~/helpers/currency";
import { DateString, getDateFromMonthsAgo } from "~/helpers/datetime";
import { AccountTypeClassification, IAccountResponse } from "~/models/account";
import { IBalanceResponse } from "~/models/balance";
import React from "react";
import { DatesRangeValue } from "@mantine/dates";
import dayjs from "dayjs";
import { useSensitiveAmountFormatter } from "~/hooks/useSensitiveAmountFormatter";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { buildValueChartData } from "../ValueChart/helpers/valueChart";
import { useAccountTypes } from "~/providers/AccountTypeProvider/AccountTypeProvider";

/**
 * Builds data for a net worth chart based on provided balances and accounts.
 *
 * @param balances An array of IBalance objects representing account balances.
 * @param accounts An array of IAccount objects representing accounts.
 * @returns An array of objects, where each object represents a date and the corresponding net worth data.
 */
const buildNetWorthChartData = (
  balances: IBalanceResponse[],
  accounts: IAccountResponse[],
  liabilityAccountTypes: string[],
  formatDateString: (date: DateString) => string,
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

  return accountChartData.map((dataPoint) => {
    const chartDataPoint: NetWorthChartData = {
      date: dataPoint.date,
      assets: 0,
      liabilities: 0,
      net: 0,
    };

    accounts.forEach((account) => {
      const accountValue = dataPoint[account.id];
      if (typeof accountValue !== "number") {
        return;
      }

      const chartIndex = liabilityAccountTypes.includes(account.type)
        ? "liabilities"
        : "assets";

      chartDataPoint[chartIndex] += accountValue;
    });

    chartDataPoint.net = chartDataPoint.assets + chartDataPoint.liabilities;
    return chartDataPoint;
  });
};

interface NetWorthChartProps {
  accounts: IAccountResponse[];
  balances: IBalanceResponse[];
  dateRange: DatesRangeValue<string>;
  isPending?: boolean;
  invertYAxis?: boolean;
}

const NetWorthChart = (props: NetWorthChartProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dateFormat } = useLocale();
  const { allAccountTypes } = useAccountTypes();
  const formatSensitiveAmount = useSensitiveAmountFormatter();

  const liabilityAccountTypes = React.useMemo(
    () =>
      allAccountTypes
        .filter(
          (type) => type.classification === AccountTypeClassification.Liability,
        )
        .map((type) => type.value),
    [allAccountTypes],
  );

  const chartData = React.useMemo(() => {
    if (props.accounts.length === 0 || props.balances.length === 0) {
      return [];
    }

    return buildNetWorthChartData(
      filterBalancesByDateRange(
        props.balances,
        props.dateRange[0]
          ? dayjs(props.dateRange[0]).toDate()
          : getDateFromMonthsAgo(1),
        dayjs(props.dateRange[1]).isValid()
          ? dayjs(props.dateRange[1]).toDate()
          : dayjs().toDate(),
      ),
      props.accounts,
      liabilityAccountTypes,
      (date: DateString) => dayjs(date).format(dateFormat),
    );
  }, [
    dateFormat,
    liabilityAccountTypes,
    props.accounts,
    props.balances,
    props.dateRange,
  ]);

  const chartValueFormatter = React.useCallback(
    (value: number) =>
      formatSensitiveAmount(value, SignDisplay.Auto, undefined, 0),
    [formatSensitiveAmount],
  );

  return (
    <BBNetWorthChart
      ariaLabel={t("net_worth")}
      data={chartData}
      emptyMessage={t("select_an_account_to_display_the_chart")}
      formatValue={chartValueFormatter}
      isLoading={props.isPending}
      seriesLabels={{
        assets: t("assets"),
        liabilities: t("liabilities"),
        net: t("net"),
      }}
    />
  );
};

export default NetWorthChart;
