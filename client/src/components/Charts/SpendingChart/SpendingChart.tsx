import { ITransaction } from "~/models/transaction";
import { AreaChart } from "@mantine/charts";
import React from "react";
import {
  buildTransactionChartData,
  buildTransactionChartSeries,
} from "~/helpers/charts";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { Group, Skeleton } from "@mantine/core";
import ChartTooltip from "../ChartTooltip/ChartTooltip";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface SpendingChartProps {
  transactions: ITransaction[];
  months: Date[];
  isPending?: boolean;
  includeGrid?: boolean;
  includeYAxis?: boolean;
  h?: number | string;
}

const SpendingChart = (props: SpendingChartProps): React.ReactNode => {
  const sortedMonths = props.months.sort((a, b) => a.getTime() - b.getTime());

  const { t } = useTranslation();
  const { dayjs, intlLocale } = useLocale();
  const { preferredCurrency } = useUserSettings();

  const formatDateString = (date: Date) => dayjs(date).format("MMMM YYYY");

  const chartData = React.useMemo(
    () =>
      buildTransactionChartData(
        sortedMonths,
        props.transactions,
        formatDateString,
      ),
    [sortedMonths, props.transactions, formatDateString],
  );

  const chartSeries = React.useMemo(
    () => buildTransactionChartSeries(sortedMonths, formatDateString),
    [sortedMonths, formatDateString],
  );

  const chartValueFormatter = (value: number): string => {
    return convertNumberToCurrency(
      value,
      false,
      preferredCurrency,
      SignDisplay.Auto,
      intlLocale,
    );
  };

  if (props.isPending) {
    return <Skeleton height={425} radius="lg" />;
  }

  if (props.months.length === 0) {
    return (
      <Group justify="center">
        <DimmedText size="sm">
          {t("select_a_month_to_display_the_chart")}
        </DimmedText>
      </Group>
    );
  }

  return (
    <AreaChart
      w="100%"
      h={props.h ?? "100%"}
      series={chartSeries}
      data={chartData}
      dataKey="day"
      valueFormatter={chartValueFormatter}
      withLegend
      tooltipAnimationDuration={200}
      tooltipProps={{
        content: ({ label, payload }) => (
          <ChartTooltip
            label={label}
            payload={payload}
            series={chartSeries}
            valueFormatter={chartValueFormatter}
          />
        ),
      }}
      curveType="monotone"
      withYAxis={props.includeYAxis}
      gridAxis={props.includeGrid ? "xy" : "none"}
    />
  );
};

export default SpendingChart;
