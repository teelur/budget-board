import {
  MonthlySpendingChart as BBMonthlySpendingChart,
  type MonthlySpendingChartData,
} from "@teelur/budget-board-ui";
import { ITransaction } from "~/models/transaction";
import React from "react";
import { buildMonthlySpendingChartData } from "~/helpers/charts";
import { SignDisplay } from "~/helpers/currency";
import { useSensitiveAmountFormatter } from "~/hooks/useSensitiveAmountFormatter";
import { useTranslation } from "react-i18next";

interface MonthlySpendingChartProps {
  transactions: ITransaction[];
  months: Date[];
  isPending?: boolean;
  includeGrid?: boolean;
  includeYAxis?: boolean;
  invertData?: boolean;
}

const MonthlySpendingChart = (
  props: MonthlySpendingChartProps,
): React.ReactNode => {
  const sortedMonths = [...props.months].sort(
    (a, b) => a.getTime() - b.getTime(),
  );

  const { t } = useTranslation();
  const formatSensitiveAmount = useSensitiveAmountFormatter();

  const chartData = React.useMemo<MonthlySpendingChartData[]>(
    () =>
      buildMonthlySpendingChartData(
        sortedMonths,
        props.transactions,
        props.invertData ?? false,
      ),
    [sortedMonths, props.transactions, props.invertData],
  );

  const average = React.useMemo(() => {
    if (chartData.length === 0) {
      return 0;
    }

    // Calculate the average total for the chart data excluding the current month
    const total = chartData.reduce((acc, data) => {
      if (
        data.month ===
        new Date().toLocaleString("default", {
          month: "numeric",
          year: "2-digit",
        })
      ) {
        return acc; // Skip the current month
      }
      return acc + (data.total ?? 0);
    }, 0);

    // Avoid division by zero
    if (chartData.length <= 1) {
      return total;
    }

    // Subtract 1 to exclude the current month from the average
    return total / (chartData.length - 1);
  }, [chartData]);

  const chartValueFormatter = React.useCallback(
    (value: number) =>
      formatSensitiveAmount(value, SignDisplay.Auto, undefined, 0),
    [formatSensitiveAmount],
  );

  return (
    <BBMonthlySpendingChart
      ariaLabel={t(props.invertData ? "expense_trends" : "income_trends")}
      average={average}
      averageLabel={
        props.invertData ? t("average_spending") : t("average_income")
      }
      data={chartData}
      emptyMessage={t("select_a_month_to_display_the_chart")}
      formatValue={chartValueFormatter}
      height={400}
      includeGrid={props.includeGrid}
      includeYAxis={props.includeYAxis}
      isLoading={props.isPending}
    />
  );
};

export default MonthlySpendingChart;
