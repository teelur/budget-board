import { ITransaction } from "~/models/transaction";
import { DonutChart } from "@mantine/charts";
import React from "react";
import { BuildSpendingCategoryChartData } from "~/helpers/charts";
import { Group, Skeleton, Text } from "@mantine/core";
import { ICategory } from "~/models/category";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface SpendingChartProps {
  transactions: ITransaction[];
  categories: ICategory[];
  isPending?: boolean;
}

const SpendingCategoriesChart = (
  props: SpendingChartProps,
): React.ReactNode => {
  const { dayjs, longDateFormat, intlLocale } = useLocale();
  const { preferredCurrency } = useUserSettings();

  if (props.isPending) {
    return <Skeleton height={425} radius="lg" />;
  }

  if (props.transactions.length === 0) {
    return (
      <Group justify="center">
        <Text>Select a month to display the chart.</Text>
      </Group>
    );
  }

  const chartData = React.useMemo(
    () => BuildSpendingCategoryChartData(props.transactions, props.categories),
    [props.transactions, props.categories],
  );

  return (
    <DonutChart
      data={chartData}
      size={425}
      thickness={40}
      withTooltip
      tooltipDataSource="segment"
      valueFormatter={(value) =>
        convertNumberToCurrency(
          value,
          true,
          preferredCurrency,
          SignDisplay.Auto,
          intlLocale,
        )
      }
    />
  );
};

export default SpendingCategoriesChart;
