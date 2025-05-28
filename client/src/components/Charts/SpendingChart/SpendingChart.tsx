import { ITransaction } from "~/models/transaction";
import { AreaChart } from "@mantine/charts";
import React from "react";
import {
  buildTransactionChartData,
  buildTransactionChartSeries,
} from "~/helpers/charts";
import { convertNumberToCurrency } from "~/helpers/currency";
import { Group, Skeleton, Text } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";

interface SpendingChartProps {
  transactions: ITransaction[];
  months: Date[];
  isPending?: boolean;
  includeGrid?: boolean;
  includeYAxis?: boolean;
}

const SpendingChart = (props: SpendingChartProps): React.ReactNode => {
  const sortedMonths = props.months.sort((a, b) => a.getTime() - b.getTime());

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
    () => buildTransactionChartData(sortedMonths, props.transactions),
    [sortedMonths, props.transactions]
  );

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
    <AreaChart
      h={400}
      w="100%"
      series={buildTransactionChartSeries(sortedMonths)}
      data={chartData}
      dataKey="day"
      valueFormatter={(value) =>
        userSettingsQuery.isPending
          ? ""
          : convertNumberToCurrency(
              value,
              true,
              userSettingsQuery.data?.currency ?? "USD"
            )
      }
      withLegend
      tooltipAnimationDuration={200}
      curveType="monotone"
      withYAxis={props.includeYAxis}
      gridAxis={props.includeGrid ? "xy" : "none"}
    />
  );
};

export default SpendingChart;
