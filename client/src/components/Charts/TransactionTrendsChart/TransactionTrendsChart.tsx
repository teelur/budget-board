import { useElementSize } from "@mantine/hooks";
import { SpendingChart as BBSpendingChart } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import {
  buildTransactionChartData,
  buildTransactionChartSeries,
} from "~/helpers/charts";
import { SignDisplay } from "~/helpers/currency";
import { useSensitiveAmountFormatter } from "~/hooks/useSensitiveAmountFormatter";
import { ITransaction } from "~/models/transaction";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import type { CategoryType } from "~/models/category";

interface TransactionTrendsChartProps {
  transactions: ITransaction[];
  months: Date[];
  isPending?: boolean;
  includeGrid?: boolean;
  includeYAxis?: boolean;
  height?: number;
  getCategoryType?: (category: string) => CategoryType | "";
}

const TransactionTrendsChart = (
  props: TransactionTrendsChartProps,
): React.ReactNode => {
  const sortedMonths = React.useMemo(
    () => [...props.months].sort((a, b) => a.getTime() - b.getTime()),
    [props.months],
  );
  const { ref: chartContainerRef, height: chartContainerHeight } =
    useElementSize<HTMLDivElement>();
  const { t } = useTranslation();
  const { dayjs } = useLocale();
  const formatSensitiveAmount = useSensitiveAmountFormatter();

  const formatDateString = React.useCallback(
    (date: Date) => dayjs(date).format("MMMM YYYY"),
    [dayjs],
  );

  const chartData = React.useMemo(
    () =>
      buildTransactionChartData(
        sortedMonths,
        props.transactions,
        formatDateString,
        props.getCategoryType,
      ),
    [sortedMonths, props.transactions, formatDateString, props.getCategoryType],
  );

  const chartSeries = React.useMemo(
    () => buildTransactionChartSeries(sortedMonths, formatDateString),
    [sortedMonths, formatDateString],
  );

  const chartValueFormatter = React.useCallback(
    (value: number) =>
      formatSensitiveAmount(value, SignDisplay.Auto, undefined, 0),
    [formatSensitiveAmount],
  );
  const chartHeight =
    props.height ??
    (chartContainerHeight > 0
      ? Math.max(1, Math.floor(chartContainerHeight))
      : 400);

  return (
    <div
      ref={chartContainerRef}
      style={{
        height: props.height ?? "100%",
        minHeight: 0,
        width: "100%",
      }}
    >
      <BBSpendingChart
        ariaLabel={t("transaction_trends")}
        data={chartData}
        emptyMessage={t("select_a_month_to_display_the_chart")}
        formatValue={chartValueFormatter}
        height={chartHeight}
        includeGrid={props.includeGrid}
        includeYAxis={props.includeYAxis}
        isLoading={props.isPending}
        series={chartSeries}
        style={{ height: "100%" }}
      />
    </div>
  );
};

export default TransactionTrendsChart;
