import { NetCashFlowChart as BBNetCashFlowChart } from "@teelur/budget-board-ui";
import { SignDisplay } from "~/helpers/currency";
import { getTransactionsForMonth } from "~/helpers/transactions";
import { areStringsEqual } from "~/helpers/utils";
import { ITransaction } from "~/models/transaction";
import React from "react";
import { useSensitiveAmountFormatter } from "~/hooks/useSensitiveAmountFormatter";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { CategoryTypes } from "~/models/category";

interface NetCashFlowChartProps {
  transactions: ITransaction[];
  months: Date[];
  isPending?: boolean;
}

const NetCashFlowChart = (props: NetCashFlowChartProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs } = useLocale();
  const { getCategoryType } = useTransactionCategories();
  const formatSensitiveAmount = useSensitiveAmountFormatter();

  const chartData = React.useMemo(
    () =>
      [...props.months]
        .sort(
          (a, b) =>
            dayjs(a).startOf("month").valueOf() -
            dayjs(b).startOf("month").valueOf(),
        )
        .map((month) => {
          const transactionsForMonth = getTransactionsForMonth(
            props.transactions,
            month,
          );

          const income = transactionsForMonth.reduce(
            (acc: number, curr: ITransaction) =>
              areStringsEqual(
                getCategoryType(curr.category ?? ""),
                CategoryTypes.Income,
              )
                ? acc + curr.amount
                : acc,
            0,
          );

          const spending = transactionsForMonth.reduce(
            (acc: number, curr: ITransaction) =>
              areStringsEqual(
                getCategoryType(curr.category ?? ""),
                CategoryTypes.Expense,
              )
                ? acc + curr.amount
                : acc,
            0,
          );

          return {
            month: dayjs(month).format("MMMM YYYY"),
            income,
            spending,
            net: income + spending,
          };
        }),
    [dayjs, getCategoryType, props.months, props.transactions, t],
  );

  const chartValueFormatter = React.useCallback(
    (value: number) =>
      formatSensitiveAmount(value, SignDisplay.Auto, undefined, 0),
    [formatSensitiveAmount],
  );

  return (
    <BBNetCashFlowChart
      ariaLabel={t("net_cash_flow")}
      data={chartData}
      emptyMessage={t("select_a_month_to_display_the_chart")}
      formatValue={chartValueFormatter}
      isLoading={props.isPending}
      seriesLabels={{
        income: t("income"),
        spending: t("spending"),
        net: t("net"),
      }}
    />
  );
};

export default NetCashFlowChart;
