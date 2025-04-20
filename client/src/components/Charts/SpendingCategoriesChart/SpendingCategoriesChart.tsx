import { ITransaction } from "~/models/transaction";
import { DonutChart } from "@mantine/charts";
import React from "react";
import { BuildSpendingCategoryChartData } from "~/helpers/charts";
import { Group, Skeleton, Text } from "@mantine/core";
import { ICategory } from "~/models/category";
import { convertNumberToCurrency } from "~/helpers/currency";

interface SpendingChartProps {
  transactions: ITransaction[];
  categories: ICategory[];
  isPending?: boolean;
}

const SpendingCategoriesChart = (
  props: SpendingChartProps
): React.ReactNode => {
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
    [props.transactions]
  );

  return (
    <DonutChart
      data={chartData}
      size={425}
      thickness={40}
      withTooltip
      tooltipDataSource="segment"
      valueFormatter={(value) => convertNumberToCurrency(value, true)}
    />
  );
};

export default SpendingCategoriesChart;
