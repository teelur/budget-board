import { ITransaction } from "~/models/transaction";
import { BarChart } from "@mantine/charts";
import React from "react";
import { buildMonthlySpendingChartData } from "~/helpers/charts";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { Group, Skeleton, Stack } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface SpendingChartProps {
  transactions: ITransaction[];
  months: Date[];
  isPending?: boolean;
  includeGrid?: boolean;
  includeYAxis?: boolean;
  invertData?: boolean;
}

const MonthlySpendingChart = (props: SpendingChartProps): React.ReactNode => {
  const sortedMonths = [...props.months].sort(
    (a, b) => a.getTime() - b.getTime(),
  );

  const { t } = useTranslation();
  const { intlLocale } = useLocale();
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

  const chartData = React.useMemo(
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

  if (props.isPending) {
    return <Skeleton height={425} radius="lg" />;
  }

  if (props.months.length === 0) {
    return (
      <Group justify="center">
        <DimmedText size="md">
          {t("select_a_month_to_display_the_chart")}
        </DimmedText>
      </Group>
    );
  }

  return (
    <Stack gap="1rem">
      <Group justify="space-between" align="center">
        <DimmedText size="sm">
          {props.invertData ? t("average_spending") : t("average_income")}
        </DimmedText>
        <DimmedText size="sm">
          {userSettingsQuery.isPending
            ? ""
            : convertNumberToCurrency(
                average,
                false,
                userSettingsQuery.data?.currency ?? "USD",
                SignDisplay.Auto,
                intlLocale,
              )}
        </DimmedText>
      </Group>
      <BarChart
        h={400}
        w="100%"
        series={[
          {
            name: "total",
            label: t("total"),
            color: "blue.6",
          },
        ]}
        data={chartData}
        dataKey="month"
        valueFormatter={(value) =>
          userSettingsQuery.isPending
            ? ""
            : convertNumberToCurrency(
                value,
                false,
                userSettingsQuery.data?.currency ?? "USD",
                SignDisplay.Auto,
                intlLocale,
              )
        }
        referenceLines={[
          {
            y: average,
            color: "red.5",
            label: "Average",
            labelPosition: "insideTopRight",
          },
        ]}
        withTooltip={false}
        tooltipAnimationDuration={200}
        xAxisProps={{ angle: -20 }}
        withBarValueLabel
        withYAxis={props.includeYAxis}
        gridAxis={props.includeGrid ? "xy" : "none"}
      />
    </Stack>
  );
};

export default MonthlySpendingChart;
