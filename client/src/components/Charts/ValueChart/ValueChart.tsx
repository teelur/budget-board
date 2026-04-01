import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { BarChart } from "@mantine/charts";
import { Group, Skeleton } from "@mantine/core";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";
import { DatesRangeValue } from "@mantine/dates";
import ChartTooltip from "~/components/Charts/ChartTooltip/ChartTooltip";
import {
  buildValueChartData,
  buildValueChartSeries,
  filterValuesByDateRange,
  IItem,
  IValue,
} from "./helpers/valueChart";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface ValueChartProps {
  items: IItem[];
  values: IValue[];
  dateRange: DatesRangeValue<string>;
  isPending?: boolean;
  invertYAxis?: boolean;
}

const ValueChart = (props: ValueChartProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs, dateFormat, intlLocale } = useLocale();
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

  const chartSeries = buildValueChartSeries(props.items);

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

  if (props.items?.length === 0 || props.values?.length === 0) {
    return (
      <Group justify="center" p="0.5rem">
        <DimmedText size="sm">{t("no_data_available")}</DimmedText>
      </Group>
    );
  }

  const sortedChartValues = () => {
    const startDate: Date = props.dateRange[0]
      ? dayjs(props.dateRange[0]).toDate()
      : dayjs().subtract(1, "month").toDate();
    const endDate: Date = props.dateRange[1]
      ? dayjs(props.dateRange[1]).toDate()
      : dayjs().toDate();

    const filteredValues: IValue[] = filterValuesByDateRange(
      props.values,
      startDate,
      endDate,
    );

    return filteredValues.sort((a, b) =>
      dayjs(a.dateTime).diff(dayjs(b.dateTime)),
    );
  };

  const formatDateString = React.useCallback(
    (date: Date): string => dayjs(date).format(dateFormat),
    [dayjs, dateFormat],
  );

  return (
    <BarChart
      h={400}
      w="100%"
      data={buildValueChartData(
        sortedChartValues(),
        formatDateString,
        props.invertYAxis,
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
