import { convertNumberToCurrency } from "~/helpers/currency";
import { getDateFromMonthsAgo } from "~/helpers/datetime";
import { BarChart } from "@mantine/charts";
import { Group, Skeleton, Text } from "@mantine/core";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";
import { DatesRangeValue } from "@mantine/dates";
import dayjs from "dayjs";
import ChartTooltip from "~/components/Charts/ChartTooltip/ChartTooltip";
import {
  buildValueChartData,
  buildValueChartSeries,
  filterValuesByDateRange,
  IItem,
  IValue,
} from "./helpers/valueChart";

interface ValueChartProps {
  items: IItem[];
  values: IValue[];
  dateRange: DatesRangeValue<string>;
  isPending?: boolean;
  invertYAxis?: boolean;
}

const ValueChart = (props: ValueChartProps): React.ReactNode => {
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

  const chartSeries = buildValueChartSeries(props.items);

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

  if (props.items?.length === 0 || props.values?.length === 0) {
    return (
      <Group justify="center">
        <Text>No data available.</Text>
      </Group>
    );
  }

  return (
    <BarChart
      h={400}
      w="100%"
      data={buildValueChartData(
        filterValuesByDateRange(
          props.values,
          props.dateRange[0]
            ? dayjs(props.dateRange[0]).toDate()
            : getDateFromMonthsAgo(1),
          props.dateRange[1] ? dayjs(props.dateRange[1]).toDate() : new Date()
        ),
        props.invertYAxis
      )}
      series={chartSeries}
      dataKey="dateString"
      type="stacked"
      withLegend
      tooltipAnimationDuration={200}
      tooltipProps={{
        content: ({ label, payload }) => (
          <ChartTooltip
            label={label}
            payload={payload}
            series={chartSeries}
            valueFormatter={chartValueFormatter}
            includeTotal
          />
        ),
      }}
      valueFormatter={chartValueFormatter}
    />
  );
};

export default ValueChart;
