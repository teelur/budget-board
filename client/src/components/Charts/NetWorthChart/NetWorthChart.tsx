import { filterBalancesByDateRange } from "~/helpers/balances";
import { convertNumberToCurrency } from "~/helpers/currency";
import { getDateFromMonthsAgo } from "~/helpers/datetime";
import { CompositeChart, CompositeChartSeries } from "@mantine/charts";
import { Group, Skeleton } from "@mantine/core";
import { IAccountResponse } from "~/models/account";
import { IBalanceResponse } from "~/models/balance";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";
import { DatesRangeValue } from "@mantine/dates";
import dayjs from "dayjs";
import ChartTooltip from "../ChartTooltip/ChartTooltip";
import { BuildNetWorthChartData } from "./helpers/netWorthChart";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { useDate } from "~/providers/DateProvider/DateProvider";

interface NetWorthChartProps {
  accounts: IAccountResponse[];
  balances: IBalanceResponse[];
  dateRange: DatesRangeValue<string>;
  isPending?: boolean;
  invertYAxis?: boolean;
}

const NetWorthChart = (props: NetWorthChartProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dateFormat } = useDate();
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

  const chartSeries: CompositeChartSeries[] = [
    { name: "assets", label: t("assets"), color: "green.6", type: "bar" },
    {
      name: "liabilities",
      label: t("liabilities"),
      color: "red.6",
      type: "bar",
    },
    { name: "net", label: t("net"), color: "gray.0", type: "line" },
  ];

  const chartValueFormatter = (value: number): string => {
    return userSettingsQuery.isPending
      ? ""
      : convertNumberToCurrency(
          value,
          false,
          userSettingsQuery.data?.currency ?? "USD",
        );
  };

  const formatDateString = (date: Date) => dayjs(date).format(dateFormat);

  if (props.isPending) {
    return <Skeleton height={425} radius="lg" />;
  }

  if (props.accounts?.length === 0 || props.balances?.length === 0) {
    return (
      <Group justify="center">
        <DimmedText size="sm">
          {t("select_an_account_to_display_the_chart")}
        </DimmedText>
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
          dayjs(props.dateRange[1]).isValid()
            ? dayjs(props.dateRange[1]).toDate()
            : dayjs().toDate(),
        ),
        props.accounts,
        formatDateString,
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
