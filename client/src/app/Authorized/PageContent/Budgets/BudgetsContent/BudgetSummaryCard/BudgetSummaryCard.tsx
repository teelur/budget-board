import { Flex, Group, Skeleton, Stack } from "@mantine/core";
import React from "react";
import BudgetSummaryItem from "./BudgetSummaryItem/BudgetSummaryItem";
import { IBudget } from "~/models/budget";
import { ICategory } from "~/models/category";
import { areStringsEqual } from "~/helpers/utils";
import { StatusColorType } from "~/helpers/budgets";
import { useTranslation } from "react-i18next";
import SplitCard, {
  BorderThickness,
} from "~/components/ui/SplitCard/SplitCard";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import { PiggyBankIcon } from "lucide-react";
import Divider from "~/components/core/Divider/Divider";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";

interface BudgetSummaryCardProps {
  incomeCategories: ICategory[];
  expenseCategories: ICategory[];
  budgets: IBudget[];
  categoryToTransactionsTotalMap: Map<string, number>;
  categoryToRecurringForecastTotalMap: Map<string, number>;
  unbudgetedIncomeCategoryTree: ICategory[];
  unbudgetedExpenseCategoryTree: ICategory[];
  projectedUnbudgetedTransactionsTotal: number;
  isPending: boolean;
  isForecastError: boolean;
}

const BudgetSummaryCard = (props: BudgetSummaryCardProps): React.ReactNode => {
  const { t } = useTranslation();

  const incomeBudgetsTotal = props.budgets
    .filter((b) =>
      props.incomeCategories.some((c) => areStringsEqual(b.category, c.value)),
    )
    .reduce((acc, b) => acc + b.limit, 0);
  const incomeTransactionsTotal = props.incomeCategories.reduce(
    (acc, category) => {
      const transactionsTotal = props.categoryToTransactionsTotalMap.get(
        category.value.toLocaleLowerCase(),
      );
      return acc + (transactionsTotal ?? 0);
    },
    0,
  );

  const expenseBudgetsTotal = props.budgets
    .filter((b) =>
      props.expenseCategories.some((c) => areStringsEqual(b.category, c.value)),
    )
    .reduce((acc, b) => acc + b.limit, 0);
  const expenseTransactionsTotal = props.expenseCategories.reduce(
    (acc, category) => {
      const transactionsTotal = props.categoryToTransactionsTotalMap.get(
        category.value.toLocaleLowerCase(),
      );
      return acc + (transactionsTotal ?? 0);
    },
    0,
  );

  const unbudgetedTransactionsTotal =
    [
      ...props.unbudgetedIncomeCategoryTree,
      ...props.unbudgetedExpenseCategoryTree,
    ]
      .map((category) => {
        const transactionsTotal = props.categoryToTransactionsTotalMap.get(
          category.value.toLocaleLowerCase(),
        );
        return transactionsTotal ?? 0;
      })
      .reduce((acc, total) => acc + total, 0) +
    (props.categoryToTransactionsTotalMap.get("") ?? 0);

  const getForecastTotal = (category: ICategory): number =>
    props.categoryToRecurringForecastTotalMap.get(
      category.value.toLocaleLowerCase(),
    ) ?? 0;
  const incomeForecastTotal = props.incomeCategories.reduce(
    (acc, category) => acc + getForecastTotal(category),
    0,
  );
  const expenseForecastTotal = props.expenseCategories.reduce(
    (acc, category) => acc + getForecastTotal(category),
    0,
  );
  const projectedIncomeTransactionsTotal = props.isForecastError
    ? undefined
    : incomeTransactionsTotal + incomeForecastTotal;
  const projectedExpenseTransactionsTotal = props.isForecastError
    ? undefined
    : expenseTransactionsTotal + expenseForecastTotal;
  const projectedUnbudgetedTotal = props.isForecastError
    ? undefined
    : unbudgetedTransactionsTotal + props.projectedUnbudgetedTransactionsTotal;
  const projectedNetExpensesTotal =
    projectedIncomeTransactionsTotal === undefined ||
    projectedExpenseTransactionsTotal === undefined
      ? undefined
      : projectedIncomeTransactionsTotal + projectedExpenseTransactionsTotal;
  const projectedNetCashFlowTotal =
    projectedNetExpensesTotal === undefined ||
    projectedUnbudgetedTotal === undefined
      ? undefined
      : projectedNetExpensesTotal + projectedUnbudgetedTotal;

  return (
    <SplitCard
      border={BorderThickness.Thick}
      header={
        <Group gap="0.25rem">
          <PiggyBankIcon color="var(--base-color-text-dimmed)" />
          <PrimaryHeading order={4} lh={1}>
            {t("budget_summary")}
          </PrimaryHeading>
        </Group>
      }
      elevation={1}
    >
      {props.isForecastError && (
        <Group gap="0.25rem" px="0.75rem" mt="0.25rem">
          <DimmedText size="xs">
            {t("error_loading_configuration_message")}
          </DimmedText>
        </Group>
      )}
      <Stack my="0.5rem" gap={0}>
        {props.isPending ? (
          <Flex h="100%" w="100%" p="0.5rem">
            <Skeleton h={100} radius="md" />
          </Flex>
        ) : (
          <Stack px="0.5rem" gap="0.25rem">
            <BudgetSummaryItem
              label={t("income")}
              amount={incomeTransactionsTotal}
              total={incomeBudgetsTotal}
              projectedAmount={projectedIncomeTransactionsTotal}
              budgetValueType={StatusColorType.Income}
              showDivider={incomeBudgetsTotal <= 0}
            />
            <BudgetSummaryItem
              label={t("expenses")}
              amount={expenseTransactionsTotal}
              total={expenseBudgetsTotal}
              projectedAmount={projectedExpenseTransactionsTotal}
              budgetValueType={StatusColorType.Expense}
              showDivider={expenseBudgetsTotal <= 0}
            />
            <BudgetSummaryItem
              label={t("net_expenses")}
              amount={incomeTransactionsTotal + expenseTransactionsTotal}
              projectedAmount={projectedNetExpensesTotal}
              budgetValueType={StatusColorType.Total}
              hideProgress
              showDivider
            />
          </Stack>
        )}
        <Divider my="0.25rem" elevation={1} />
        {props.isPending ? (
          <Flex h="100%" w="100%" p="0.5rem">
            <Skeleton h={43} radius="md" />
          </Flex>
        ) : (
          <Stack px="0.5rem" gap="0.25rem">
            <BudgetSummaryItem
              label={t("unbudgeted")}
              amount={unbudgetedTransactionsTotal}
              projectedAmount={projectedUnbudgetedTotal}
              budgetValueType={StatusColorType.Total}
              hideProgress
              showDivider
            />
          </Stack>
        )}
        <Divider my="0.25rem" elevation={1} />
        {props.isPending ? (
          <Flex h="100%" w="100%" p="0.5rem">
            <Skeleton h={43} radius="md" />
          </Flex>
        ) : (
          <Stack px="0.5rem" gap="0.25rem">
            <BudgetSummaryItem
              label={t("net_cash_flow")}
              amount={
                incomeTransactionsTotal +
                expenseTransactionsTotal +
                unbudgetedTransactionsTotal
              }
              projectedAmount={projectedNetCashFlowTotal}
              budgetValueType={StatusColorType.Total}
              hideProgress
              showDivider
            />
          </Stack>
        )}
      </Stack>
    </SplitCard>
  );
};

export default BudgetSummaryCard;
