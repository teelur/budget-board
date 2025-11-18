import { ITransaction } from "~/models/transaction";
import { BarChart } from "@mantine/charts";
import React from "react";
import { buildMonthlySpendingChartData } from "~/helpers/charts";
import { convertNumberToCurrency } from "~/helpers/currency";
import { Group, Skeleton, Stack, Text } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { AuthContext } from "~/providers/AuthProvider/AuthProvider";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";

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
    (a, b) => a.getTime() - b.getTime()
  );

  const { request } = React.useContext<any>(AuthContext);

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
        props.invertData ?? false
      ),
    [sortedMonths, props.transactions, props.invertData]
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
        <Text>Select a month to display the chart.</Text>
      </Group>
    );
  }

  return (
    <Stack gap="1rem">
      <Group justify="space-between" align="center">
        <Text size="sm" fw={500}>
          {props.invertData ? "Average Spending" : "Average Income"}
        </Text>
        <Text size="sm" fw={600}>
          {userSettingsQuery.isPending
            ? ""
            : convertNumberToCurrency(
                average,
                false,
                userSettingsQuery.data?.currency ?? "USD"
              )}
        </Text>
      </Group>
      <BarChart
        h={400}
        w="100%"
        series={[
          {
            name: "total",
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
                userSettingsQuery.data?.currency ?? "USD"
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
