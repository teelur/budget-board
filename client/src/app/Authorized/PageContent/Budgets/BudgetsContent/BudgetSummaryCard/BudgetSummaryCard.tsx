import { Skeleton, Stack } from "@mantine/core";
import React from "react";
import BudgetSummaryItem from "./BudgetSummaryItem/BudgetSummaryItem";
import { IBudget } from "~/models/budget";
import { ITransaction } from "~/models/transaction";
import { ICategory } from "~/models/category";
import { areStringsEqual } from "~/helpers/utils";
import { sumTransactionAmounts } from "~/helpers/transactions";
import { StatusColorType } from "~/helpers/budgets";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";

interface BudgetSummaryCardProps {
  incomeCategories: ICategory[];
  expenseCategories: ICategory[];
  budgets: IBudget[];
  transactions: ITransaction[];
  categoryToTransactionsTotalMap: Map<string, number>;
  unbudgetedCategoryTree: ICategory[];
  isPending: boolean;
}

const BudgetSummaryCard = (props: BudgetSummaryCardProps): React.ReactNode => {
  const { t } = useTranslation();

  const incomeBudgetsTotal = props.budgets
    .filter((b) => areStringsEqual(b.category, "Income"))
    .reduce((acc, b) => acc + b.limit, 0);
  const incomeTransactionsTotal = props.incomeCategories.reduce(
    (acc, category) => {
      const transactionsTotal = props.categoryToTransactionsTotalMap.get(
        category.value.toLocaleLowerCase()
      );
      return acc + (transactionsTotal ?? 0);
    },
    0
  );

  const expenseBudgetsTotal = props.budgets
    .filter((b) =>
      props.expenseCategories.some((c) => areStringsEqual(b.category, c.value))
    )
    .reduce((acc, b) => acc + b.limit, 0);
  const expenseTransactionsTotal = props.expenseCategories.reduce(
    (acc, category) => {
      const transactionsTotal = props.categoryToTransactionsTotalMap.get(
        category.value.toLocaleLowerCase()
      );
      return acc + (transactionsTotal ?? 0);
    },
    0
  );

  const unbudgetedTransactionsTotal =
    props.unbudgetedCategoryTree
      .map((category) => {
        const transactionsTotal = props.categoryToTransactionsTotalMap.get(
          category.value.toLocaleLowerCase()
        );
        return transactionsTotal ?? 0;
      })
      .reduce((acc, total) => acc + total, 0) +
    (props.categoryToTransactionsTotalMap.get("") ?? 0);

  const totalTransactionsTotal = sumTransactionAmounts(props.transactions);

  return (
    <Stack gap="0.5rem">
      <PrimaryText size="md" px="0.5rem">
        {t("budget_summary")}
      </PrimaryText>
      <Card elevation={1}>
        <Stack gap="0.5rem">
          {props.isPending ? (
            <Skeleton h={105} radius="md" />
          ) : (
            <Card elevation={2}>
              <Stack gap="0.25rem">
                <BudgetSummaryItem
                  label={t("income")}
                  amount={incomeTransactionsTotal}
                  total={incomeBudgetsTotal}
                  budgetValueType={StatusColorType.Income}
                />
                <BudgetSummaryItem
                  label={t("expenses")}
                  amount={expenseTransactionsTotal}
                  total={expenseBudgetsTotal}
                  budgetValueType={StatusColorType.Expense}
                />
                <BudgetSummaryItem
                  label={t("net_expenses")}
                  amount={incomeTransactionsTotal + expenseTransactionsTotal}
                  budgetValueType={StatusColorType.Total}
                  hideProgress
                  showDivider
                />
              </Stack>
            </Card>
          )}
          {props.isPending ? (
            <Skeleton h={56} radius="md" />
          ) : (
            <Card elevation={2}>
              <Stack gap="0.25rem">
                <BudgetSummaryItem
                  label={t("unbudgeted")}
                  amount={unbudgetedTransactionsTotal}
                  budgetValueType={StatusColorType.Total}
                  hideProgress
                  showDivider
                />
                <BudgetSummaryItem
                  label={t("net_cash_flow")}
                  amount={totalTransactionsTotal}
                  budgetValueType={StatusColorType.Total}
                  hideProgress
                  showDivider
                />
              </Stack>
            </Card>
          )}
        </Stack>
      </Card>
    </Stack>
  );
};

export default BudgetSummaryCard;
