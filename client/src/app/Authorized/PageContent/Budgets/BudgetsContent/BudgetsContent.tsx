import { Group, Skeleton, Stack } from "@mantine/core";
import { IBudget } from "~/models/budget";
import { buildCategoriesTree, getParentCategory } from "~/helpers/category";
import { CategoryTypes } from "~/models/category";
import { ITransaction } from "~/models/transaction";
import { buildCategoryToTransactionsTotalMap } from "~/helpers/transactions";
import BudgetsGroupHeader from "./BudgetGroupHeader/BudgetsGroupHeader";
import BudgetSummaryCard from "./BudgetSummaryCard/BudgetSummaryCard";
import BudgetsGroup from "./BudgetsGroup/BudgetsGroup";
import UnbudgetedGroup from "./UnbudgetedGroup/UnbudgetedGroup";
import { areStringsEqual } from "~/helpers/utils";
import FixParentBudgetButton from "./FixParentBudgetButton/FixParentBudgetButton";
import BudgetDetails from "./BudgetDetails/BudgetDetails";
import React from "react";
import { useDisclosure } from "@mantine/hooks";
import { useTranslation } from "react-i18next";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";

interface BudgetsContentProps {
  budgets: IBudget[];
  transactions: ITransaction[];
  selectedDate: Date | null;
  isPending?: boolean;
}

const BudgetsContent = (props: BudgetsContentProps) => {
  const [opened, { open, close }] = useDisclosure(false);
  const [selectedCategory, setSelectedCategory] = React.useState<string | null>(
    null,
  );
  const [selectedMonth, setSelectedMonth] = React.useState<Date | null>(null);

  const { t } = useTranslation();
  const { allTransactionCategories, getCategoryType } =
    useTransactionCategories();

  const categoryToTransactionsTotalMap: Map<string, number> =
    buildCategoryToTransactionsTotalMap(props.transactions);

  const categoryTree = buildCategoriesTree(allTransactionCategories);

  const incomeBudgets = props.budgets.filter((budget) =>
    areStringsEqual(getCategoryType(budget.category), CategoryTypes.Income),
  );
  const incomeCategoryTree = categoryTree.filter(
    (category) =>
      areStringsEqual(category.categoryType, CategoryTypes.Income) &&
      props.budgets.some((budget) =>
        areStringsEqual(budget.category, category.value),
      ),
  );

  const expenseBudgets = props.budgets.filter((budget) =>
    areStringsEqual(getCategoryType(budget.category), CategoryTypes.Expense),
  );
  const expenseCategoryTree = categoryTree.filter(
    (category) =>
      areStringsEqual(category.categoryType, CategoryTypes.Expense) &&
      props.budgets.some((budget) =>
        areStringsEqual(budget.category, category.value),
      ),
  );

  const unbudgetedCategoryTree = categoryTree.filter(
    (category) =>
      !props.budgets.some((budget) =>
        areStringsEqual(
          getParentCategory(budget.category, allTransactionCategories),
          getParentCategory(category.value, allTransactionCategories),
        ),
      ) &&
      categoryToTransactionsTotalMap.has(category.value.toLocaleLowerCase()),
  );

  const unbudgetedIncomeCategoryTree = unbudgetedCategoryTree.filter((c) =>
    areStringsEqual(c.categoryType, CategoryTypes.Income),
  );
  const unbudgetedExpenseCategoryTree = unbudgetedCategoryTree.filter((c) =>
    areStringsEqual(c.categoryType, CategoryTypes.Expense),
  );

  const openBudgetDetails = (category: string, month: Date | null) => {
    open();
    setSelectedCategory(category);
    setSelectedMonth(month);
  };

  return (
    <Group gap="0.5rem" align="flex-start">
      <BudgetDetails
        isOpen={opened}
        close={close}
        category={selectedCategory ?? null}
        month={selectedMonth}
      />
      <Stack w={{ base: "100%", md: "70%" }}>
        <Stack gap="0.5rem">
          <BudgetsGroupHeader groupName={t("income")} />
          {props.isPending ? (
            <Skeleton h={65} radius="md" />
          ) : (
            <BudgetsGroup
              budgets={incomeBudgets}
              categoryTree={incomeCategoryTree}
              categoryToTransactionsTotalMap={categoryToTransactionsTotalMap}
              selectedDate={props.selectedDate}
              openDetails={openBudgetDetails}
            />
          )}
          {props.isPending ? (
            <Skeleton h={65} radius="md" />
          ) : (
            <UnbudgetedGroup
              categoryTree={unbudgetedIncomeCategoryTree}
              categoryToTransactionsTotalMap={categoryToTransactionsTotalMap}
              selectedDate={props.selectedDate}
              openDetails={openBudgetDetails}
            />
          )}
        </Stack>
        <Stack gap="0.5rem">
          <BudgetsGroupHeader groupName={t("expenses")} />
          {props.isPending ? (
            <Skeleton h={65} radius="md" />
          ) : (
            <BudgetsGroup
              budgets={expenseBudgets}
              categoryTree={expenseCategoryTree}
              categoryToTransactionsTotalMap={categoryToTransactionsTotalMap}
              selectedDate={props.selectedDate}
              openDetails={openBudgetDetails}
            />
          )}
          {props.isPending ? (
            <Skeleton h={65} radius="md" />
          ) : (
            <UnbudgetedGroup
              categoryTree={unbudgetedExpenseCategoryTree}
              categoryToTransactionsTotalMap={categoryToTransactionsTotalMap}
              selectedDate={props.selectedDate}
              openDetails={openBudgetDetails}
              showUncategorized
            />
          )}
        </Stack>
      </Stack>
      <Stack
        style={{ flexGrow: 1 }}
        w={{ base: "100%", md: "20%" }}
        h={{ base: "auto", md: "100%" }}
      >
        <BudgetSummaryCard
          incomeCategories={incomeCategoryTree}
          expenseCategories={expenseCategoryTree}
          budgets={props.budgets}
          categoryToTransactionsTotalMap={categoryToTransactionsTotalMap}
          unbudgetedIncomeCategoryTree={unbudgetedIncomeCategoryTree}
          unbudgetedExpenseCategoryTree={unbudgetedExpenseCategoryTree}
          isPending={props.isPending ?? false}
        />
        <FixParentBudgetButton
          budgets={props.budgets}
          categoryTree={categoryTree}
          categoryToTransactionsTotalMap={categoryToTransactionsTotalMap}
        />
      </Stack>
    </Group>
  );
};

export default BudgetsContent;
