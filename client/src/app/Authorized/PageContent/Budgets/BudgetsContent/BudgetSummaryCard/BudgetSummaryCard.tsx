import classes from "./BudgetTotalCard.module.css";

import { Card, Skeleton, Stack, Text } from "@mantine/core";
import React from "react";
import BudgetSummaryItem from "./BudgetSummaryItem/BudgetSummaryItem";
import { IBudget } from "~/models/budget";
import { ITransaction } from "~/models/transaction";
import { ICategory } from "~/models/category";
import { areStringsEqual } from "~/helpers/utils";
import { sumTransactionAmounts } from "~/helpers/transactions";
import { BudgetValueType } from "~/helpers/budgets";

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
      <Text fw={600} px="0.5rem">
        Your Budget
      </Text>
      <Card
        className={classes.root}
        bg="var(--mantine-color-content)"
        radius="md"
        withBorder
      >
        {props.isPending ? (
          <Skeleton h={105} radius="md" />
        ) : (
          <Card className={classes.group} radius="md">
            <BudgetSummaryItem
              label="Income"
              amount={incomeTransactionsTotal}
              total={incomeBudgetsTotal}
              budgetValueType={BudgetValueType.Income}
            />
            <BudgetSummaryItem
              label="Expenses"
              amount={expenseTransactionsTotal}
              total={expenseBudgetsTotal}
              budgetValueType={BudgetValueType.Expense}
            />
            <BudgetSummaryItem
              label="Net Budgeted"
              amount={incomeTransactionsTotal + expenseTransactionsTotal}
              budgetValueType={BudgetValueType.Total}
              hideProgress
              showDivider
            />
          </Card>
        )}
        {props.isPending ? (
          <Skeleton h={56} radius="md" />
        ) : (
          <Card className={classes.group} radius="md">
            <BudgetSummaryItem
              label="Unbudgeted"
              amount={unbudgetedTransactionsTotal}
              budgetValueType={BudgetValueType.Total}
              hideProgress
              showDivider
            />
            <BudgetSummaryItem
              label="Net Cash Flow"
              amount={totalTransactionsTotal}
              budgetValueType={BudgetValueType.Total}
              hideProgress
              showDivider
            />
          </Card>
        )}
      </Card>
    </Stack>
  );
};

export default BudgetSummaryCard;
