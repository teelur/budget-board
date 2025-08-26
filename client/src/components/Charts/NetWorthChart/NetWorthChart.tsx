import { filterBalancesByDateRange } from "~/helpers/balances";
import { BuildNetWorthChartData } from "~/helpers/charts";
import { convertNumberToCurrency } from "~/helpers/currency";
import { getDateFromMonthsAgo } from "~/helpers/datetime";
import { CompositeChart, CompositeChartSeries } from "@mantine/charts";
import { Group, Skeleton, Text } from "@mantine/core";
import { IAccount } from "~/models/account";
import { IBalance } from "~/models/balance";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";
import { DatesRangeValue } from "@mantine/dates";
import dayjs from "dayjs";
import ChartTooltip from "../ChartTooltip/ChartTooltip";

interface NetWorthChartProps {
  accounts: IAccount[];
  balances: IBalance[];
  dateRange: DatesRangeValue<string>;
  isPending?: boolean;
  invertYAxis?: boolean;
}

const NetWorthChart = (props: NetWorthChartProps): React.ReactNode => {
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

  const chartSeries: CompositeChartSeries[] = [
    { name: "assets", label: "Assets", color: "green.6", type: "bar" },
    {
      name: "liabilities",
      label: "Liabilities",
      color: "red.6",
      type: "bar",
    },
    { name: "net", label: "Net", color: "gray.0", type: "line" },
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

  if (props.accounts?.length === 0 || props.balances?.length === 0) {
    return (
      <Group justify="center">
        <Text>Select an account to display the chart.</Text>
      </Group>
    );
  }

  return (
    <CompositeChart
      h={400}
      w="100%"
      data={BuildNetWorthChartData(
        filterBalancesByDateRange(
          props.balances,
          props.dateRange[0]
            ? dayjs(props.dateRange[0]).toDate()
            : getDateFromMonthsAgo(1),
          props.dateRange[1] ? dayjs(props.dateRange[1]).toDate() : new Date()
        ),
        props.accounts
      )}
      series={chartSeries}
      withLegend
      dataKey="dateString"
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

export default NetWorthChart;
