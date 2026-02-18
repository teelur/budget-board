import { convertNumberToCurrency } from "~/helpers/currency";
import { getTransactionsForMonth } from "~/helpers/transactions";
import { areStringsEqual } from "~/helpers/utils";
import { CompositeChart, CompositeChartSeries } from "@mantine/charts";
import { Group, Skeleton } from "@mantine/core";
import { ITransaction } from "~/models/transaction";
import React from "react";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";
import ChartTooltip from "../ChartTooltip/ChartTooltip";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { useDate } from "~/providers/DateProvider/DateProvider";

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
  const { t } = useTranslation();
  const { dayjs } = useDate();
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

  const sortedMonths = props.months.sort(
    (a, b) =>
      dayjs(a).startOf("month").valueOf() - dayjs(b).startOf("month").valueOf(),
  );

  const buildChartData = (): ChartDatum[] => {
    const spendingTrendsChartData: ChartDatum[] = [];

    sortedMonths.forEach((month) => {
      const transactionsForMonth = getTransactionsForMonth(
        props.transactions,
        month,
      );

      const incomeTotal = transactionsForMonth.reduce(
        (acc: number, curr: ITransaction) =>
          areStringsEqual(curr.category ?? "", "Income")
            ? acc + curr.amount
            : acc,
        0,
      );

      const spendingTotal = transactionsForMonth.reduce(
        (acc: number, curr: ITransaction) =>
          !areStringsEqual(curr.category ?? "", "Income")
            ? acc + curr.amount
            : acc,
        0,
      );

      spendingTrendsChartData.push({
        month: dayjs(month).format("MMMM YYYY"),
        Income: incomeTotal,
        Spending: spendingTotal,
        Net: incomeTotal + spendingTotal,
      });
    });
    return spendingTrendsChartData;
  };

  const chartSeries: CompositeChartSeries[] = [
    { name: "Income", label: t("income"), color: "green.6", type: "bar" },
    { name: "Spending", label: t("spending"), color: "red.6", type: "bar" },
    { name: "Net", label: t("net"), color: "gray.0", type: "line" },
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
