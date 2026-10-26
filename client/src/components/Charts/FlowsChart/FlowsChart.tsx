import { FlowsChart as BBFlowsChart } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import { buildFlowsChartData } from "~/helpers/charts";
import { useSensitiveAmountFormatter } from "~/hooks/useSensitiveAmountFormatter";
import { SignDisplay } from "~/helpers/currency";
import { ICategory } from "~/models/category";
import { ITransaction } from "~/models/transaction";

interface FlowsChartProps {
  transactions: ITransaction[];
  categories: ICategory[];
  isPending?: boolean;
  height?: number;
  minScrollWidth?: number;
}

const FlowsChart = (props: FlowsChartProps): React.ReactNode => {
  const { t } = useTranslation();
  const formatSensitiveAmount = useSensitiveAmountFormatter();

  const chartData = React.useMemo(
    () =>
      buildFlowsChartData(props.transactions, props.categories, {
        cashAvailable: t("cash_available"),
        surplus: t("surplus"),
        deficit: t("deficit"),
        total: t("total"),
        income: t("income"),
        expense: t("expense"),
        uncategorized: t("uncategorized"),
      }),
    [props.transactions, props.categories, t],
  );

  const valueFormatter = React.useCallback(
    (value: number) =>
      formatSensitiveAmount(value, SignDisplay.Auto, undefined, 0),
    [formatSensitiveAmount],
  );
  const chartHeight = props.height ?? 750;

  return (
    <BBFlowsChart
      ariaLabel={t("flows")}
      data={chartData}
      emptyMessage={t("no_flow_data")}
      formatValue={valueFormatter}
      height={chartHeight}
      minScrollWidth={props.minScrollWidth}
      isLoading={props.isPending ?? false}
      labels={{
        cashAvailable: t("cash_available"),
        amount: t("amount"),
        from: t("from"),
        to: t("to"),
        percentOfIncome: t("percent_of_income"),
        percentOfSpending: t("percent_of_spending"),
        percentOfBranch: t("percent_of_branch"),
        transactions: t("transactions"),
        income: t("income"),
        expense: t("expense"),
        surplus: t("surplus"),
        deficit: t("deficit"),
      }}
    />
  );
};

export default FlowsChart;
