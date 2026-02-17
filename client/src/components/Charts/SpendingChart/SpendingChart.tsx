import { ITransaction } from "~/models/transaction";
import { AreaChart } from "@mantine/charts";
import React from "react";
import {
  buildTransactionChartData,
  buildTransactionChartSeries,
} from "~/helpers/charts";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { Group, Skeleton } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";
import ChartTooltip from "../ChartTooltip/ChartTooltip";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface SpendingChartProps {
  transactions: ITransaction[];
  months: Date[];
  isPending?: boolean;
  includeGrid?: boolean;
  includeYAxis?: boolean;
}

const SpendingChart = (props: SpendingChartProps): React.ReactNode => {
  const sortedMonths = props.months.sort((a, b) => a.getTime() - b.getTime());

  const { t } = useTranslation();
  const { dayjs, intlLocale } = useLocale();
  const { request } = useAuth();

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

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
    return userSettingsQuery.isPending
      ? ""
      : convertNumberToCurrency(
          value,
          false,
          userSettingsQuery.data?.currency ?? "USD",
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
      h={400}
      w="100%"
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
