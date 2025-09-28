import { convertNumberToCurrency } from "~/helpers/currency";
import { getMonthAndYearDateString } from "~/helpers/datetime";
import { getTransactionsForMonth } from "~/helpers/transactions";
import { areStringsEqual } from "~/helpers/utils";
import { CompositeChart, CompositeChartSeries } from "@mantine/charts";
import { Group, Skeleton, Text } from "@mantine/core";
import { ITransaction } from "~/models/transaction";
import React from "react";
import { useQuery } from "@tanstack/react-query";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";
import ChartTooltip from "../ChartTooltip/ChartTooltip";

interface ChartDatum {
  month: string;
  Income: number;
  Spending: number;
  Net: number;
}

interface NetCashFlowChartProps {
  transactions: ITransaction[];
  months: Date[];
  isPending?: boolean;
  includeGrid?: boolean;
  includeYAxis?: boolean;
}

const NetCashFlowChart = (props: NetCashFlowChartProps): React.ReactNode => {
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

  const sortedMonths = props.months.sort(
    (a, b) => new Date(a).getTime() - new Date(b).getTime()
  );

  const buildChartData = (): ChartDatum[] => {
    const spendingTrendsChartData: ChartDatum[] = [];

    sortedMonths.forEach((month) => {
      const transactionsForMonth = getTransactionsForMonth(
        props.transactions,
        month
      );

      const incomeTotal = transactionsForMonth.reduce(
        (acc: number, curr: ITransaction) =>
          areStringsEqual(curr.category ?? "", "Income")
            ? acc + curr.amount
            : acc,
        0
      );

      const spendingTotal = transactionsForMonth.reduce(
        (acc: number, curr: ITransaction) =>
          !areStringsEqual(curr.category ?? "", "Income")
            ? acc + curr.amount
            : acc,
        0
      );

      spendingTrendsChartData.push({
        month: getMonthAndYearDateString(month),
        Income: incomeTotal,
        Spending: spendingTotal,
        Net: incomeTotal + spendingTotal,
      });
    });
    return spendingTrendsChartData;
  };

  const chartSeries: CompositeChartSeries[] = [
    { name: "Income", color: "green.6", type: "bar" },
    { name: "Spending", color: "red.6", type: "bar" },
    { name: "Net", color: "gray.0", type: "line" },
  ];

  const chartValueFormatter = (value: number): string => {
    return userSettingsQuery.isPending
      ? ""
      : convertNumberToCurrency(
          value,
          false,
          userSettingsQuery.data?.currency ?? "USD"
        );
  };

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
    <CompositeChart
      h={400}
      w="100%"
      data={buildChartData()}
      series={chartSeries}
      withLegend
      dataKey="month"
      composedChartProps={{ stackOffset: "sign" }}
      barProps={{
        stackId: "stack",
        fillOpacity: 0.4,
        strokeOpacity: 1,
      }}
      lineProps={{ type: "linear" }}
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
      valueFormatter={chartValueFormatter}
    />
  );
};

export default NetCashFlowChart;
