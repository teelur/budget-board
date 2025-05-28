import { filterBalancesByDateRange } from "~/helpers/balances";
import {
  buildAccountBalanceChartData,
  buildAccountBalanceChartSeries,
} from "~/helpers/charts";
import { convertNumberToCurrency } from "~/helpers/currency";
import { getDateFromMonthsAgo } from "~/helpers/datetime";
import { BarChart } from "@mantine/charts";
import { Group, Skeleton, Text } from "@mantine/core";
import { DateValue } from "@mantine/dates";
import { IAccount } from "~/models/account";
import { IBalance } from "~/models/balance";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";

interface BalanceChartProps {
  accounts: IAccount[];
  balances: IBalance[];
  dateRange: [DateValue, DateValue];
  isPending?: boolean;
  invertYAxis?: boolean;
}

const BalanceChart = (props: BalanceChartProps): React.ReactNode => {
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
    <BarChart
      h={400}
      w="100%"
      data={buildAccountBalanceChartData(
        filterBalancesByDateRange(
          props.balances,
          props.dateRange[0] ?? getDateFromMonthsAgo(1),
          props.dateRange[1] ?? new Date()
        ),
        props.invertYAxis
      )}
      series={buildAccountBalanceChartSeries(props.accounts)}
      dataKey="dateString"
      type="stacked"
      withLegend
      tooltipAnimationDuration={200}
      valueFormatter={(value) =>
        userSettingsQuery.isPending
          ? ""
          : convertNumberToCurrency(
              value,
              true,
              userSettingsQuery.data?.currency ?? "USD"
            )
      }
    />
  );
};

export default BalanceChart;
