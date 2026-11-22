import { useElementSize } from "@mantine/hooks";
import {
  TransactionTrendsChart as BBTransactionTrendsChart,
  type TransactionTrendsChartSeries,
} from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import {
  buildTransactionChartData,
  buildTransactionChartSeries,
} from "~/helpers/charts";
import { SignDisplay } from "~/helpers/currency";
import { useSensitiveAmountFormatter } from "~/hooks/useSensitiveAmountFormatter";
import type { CategoryType } from "~/models/category";
import type { IRecurringForecastOccurrence } from "~/models/recurringRule";
import { ITransaction } from "~/models/transaction";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface TransactionTrendsChartProps {
  transactions: ITransaction[];
  projectedOccurrences?: IRecurringForecastOccurrence[];
  months: Date[];
  isPending?: boolean;
  includeProjections?: boolean;
  includeGrid?: boolean;
  includeYAxis?: boolean;
  height?: number;
  getCategoryType?: (category: string) => CategoryType | "";
}

const transactionMonthDifferenceKey = "transactionMonthDifference";

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
  const getProjectionSeriesKey = React.useCallback(
    (seriesKey: string) => `${seriesKey}::projected`,
    [],
  );
  const currentMonthLabel = React.useMemo(() => {
    const currentMonth = sortedMonths.find((month) =>
      dayjs(month).isSame(dayjs(), "month"),
    );
    return currentMonth ? formatDateString(currentMonth) : undefined;
  }, [dayjs, formatDateString, sortedMonths]);
  const hasProjection =
    props.includeProjections === true &&
    currentMonthLabel !== undefined &&
    dayjs().date() < dayjs().daysInMonth();

  const chartData = React.useMemo(() => {
    const transactionChartData = buildTransactionChartData(
      sortedMonths,
      props.transactions,
      formatDateString,
      props.getCategoryType,
      hasProjection ? props.projectedOccurrences : [],
      hasProjection ? getProjectionSeriesKey : undefined,
    );

    if (sortedMonths.length !== 2) {
      return transactionChartData;
    }

    const olderMonth = sortedMonths[0];
    const newerMonth = sortedMonths[1];
    if (olderMonth === undefined || newerMonth === undefined) {
      throw new Error(
        "Expected two selected months for transaction comparison.",
      );
    }

    const olderMonthKey = formatDateString(olderMonth);
    const newerMonthKey = formatDateString(newerMonth);
    const olderMonthTotalsByDay = new Map<number, number>();
    let olderMonthLastDay = 0;

    transactionChartData.forEach((dataPoint) => {
      const amount = dataPoint[olderMonthKey];
      if (amount !== undefined) {
        olderMonthTotalsByDay.set(dataPoint.day, amount);
        olderMonthLastDay = Math.max(olderMonthLastDay, dataPoint.day);
      }
    });

    if (olderMonthLastDay === 0) {
      throw new Error("No earlier-month totals are available for comparison.");
    }

    return transactionChartData.map((dataPoint) => {
      const newerMonthTotal =
        dataPoint[newerMonthKey] ??
        dataPoint[getProjectionSeriesKey(newerMonthKey)];
      if (newerMonthTotal === undefined) {
        return dataPoint;
      }

      const olderMonthDay = Math.min(dataPoint.day, olderMonthLastDay);
      const olderMonthTotal = olderMonthTotalsByDay.get(olderMonthDay);
      if (olderMonthTotal === undefined) {
        throw new Error(
          `No earlier-month total is available for day ${olderMonthDay}.`,
        );
      }

      return {
        ...dataPoint,
        [transactionMonthDifferenceKey]: newerMonthTotal - olderMonthTotal,
      };
    });
  }, [
    formatDateString,
    getProjectionSeriesKey,
    hasProjection,
    props.getCategoryType,
    props.projectedOccurrences,
    props.transactions,
    sortedMonths,
  ]);

  const chartSeries = React.useMemo(() => {
    const actualSeries: (TransactionTrendsChartSeries & {
      colorIndex: number;
    })[] = buildTransactionChartSeries(sortedMonths, formatDateString).map(
      (series, index) => ({
        ...series,
        colorIndex: index,
      }),
    );
    let seriesWithProjection = actualSeries;

    if (hasProjection && currentMonthLabel !== undefined) {
      const currentSeries = actualSeries.find(
        (series) => series.key === currentMonthLabel,
      );
      if (currentSeries !== undefined) {
        seriesWithProjection = [
          ...actualSeries,
          {
            key: getProjectionSeriesKey(currentMonthLabel),
            label: `${currentMonthLabel} (${t("projected")})`,
            colorIndex: currentSeries.colorIndex,
            legendLabel: currentMonthLabel,
            strokeDasharray: "6 4",
            type: "line",
          },
        ];
      }
    }

    return seriesWithProjection;
  }, [
    currentMonthLabel,
    formatDateString,
    getProjectionSeriesKey,
    hasProjection,
    sortedMonths,
    t,
  ]);

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
      style={{ height: props.height ?? "100%", minHeight: 0, width: "100%" }}
    >
      <BBTransactionTrendsChart
        ariaLabel={t("transaction_trends")}
        data={chartData}
        emptyMessage={t("select_a_month_to_display_the_chart")}
        formatValue={chartValueFormatter}
        height={chartHeight}
        includeGrid={props.includeGrid}
        includeYAxis={props.includeYAxis !== false}
        isLoading={props.isPending}
        tooltipSummary={
          sortedMonths.length === 2
            ? (dataPoint) => {
                const difference = dataPoint[transactionMonthDifferenceKey];
                if (difference === undefined) {
                  return null;
                }

                const differenceKey =
                  difference < 0
                    ? "transaction_trends_difference_less"
                    : difference > 0
                      ? "transaction_trends_difference_more"
                      : "transaction_trends_difference";

                return t(differenceKey, {
                  amount: formatSensitiveAmount(
                    Math.abs(difference),
                    SignDisplay.Auto,
                  ),
                });
              }
            : undefined
        }
        series={chartSeries}
        style={{ height: "100%" }}
      />
    </div>
  );
};

export default TransactionTrendsChart;
