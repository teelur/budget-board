import { ValueChart as BBValueChart } from "@teelur/budget-board-ui";
import { SignDisplay } from "~/helpers/currency";
import React from "react";
import { DatesRangeValue } from "@mantine/dates";
import { useSensitiveAmountFormatter } from "~/hooks/useSensitiveAmountFormatter";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { DateString } from "~/helpers/datetime";
import {
  buildValueChartData,
  buildValueChartSeries,
  type IItem,
  type IValue,
} from "./helpers/valueChart";

interface ValueChartProps {
  items: IItem[];
  values: IValue[];
  dateRange: DatesRangeValue<string>;
  isPending?: boolean;
  invertYAxis?: boolean;
}

const ValueChart = (props: ValueChartProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs, dateFormat } = useLocale();
  const formatSensitiveAmount = useSensitiveAmountFormatter();

  const chartSeries = React.useMemo(
    () => buildValueChartSeries(props.items),
    [props.items],
  );

  const dateLabelFormatter = React.useCallback(
    (date: DateString): string => dayjs(date).format(dateFormat),
    [dayjs, dateFormat],
  );

  const sortedChartValues = React.useMemo(() => {
    const startDate: Date = props.dateRange[0]
      ? dayjs(props.dateRange[0]).toDate()
      : dayjs().subtract(1, "month").toDate();
    const endDate: Date = props.dateRange[1]
      ? dayjs(props.dateRange[1]).toDate()
      : dayjs().toDate();

    return props.values
      .filter((value) =>
        dayjs(value.date).isBetween(startDate, endDate, "date", "[]"),
      )
      .sort((a, b) => dayjs(a.date).diff(dayjs(b.date)));
  }, [props.values, props.dateRange, dayjs]);

  const chartData = React.useMemo(
    () =>
      buildValueChartData(
        sortedChartValues,
        dateLabelFormatter,
        props.invertYAxis,
      ),
    [dateLabelFormatter, props.invertYAxis, sortedChartValues],
  );

  const chartValueFormatter = React.useCallback(
    (value: number) =>
      formatSensitiveAmount(value, SignDisplay.Auto, undefined, 0),
    [formatSensitiveAmount],
  );

  return (
    <BBValueChart
      ariaLabel={t("values")}
      data={chartData}
      emptyMessage={t("no_data_available")}
      formatValue={chartValueFormatter}
      isLoading={props.isPending}
      series={chartSeries}
      totalLabel={t("total")}
    />
  );
};

export default ValueChart;
