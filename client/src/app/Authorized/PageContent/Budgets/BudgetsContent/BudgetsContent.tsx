import { Group, Skeleton, Stack } from "@mantine/core";
import { IBudget } from "~/models/budget";
import { buildCategoriesTree, getParentCategory } from "~/helpers/category";
import { ICategory } from "~/models/category";
import { ITransaction } from "~/models/transaction";
import { buildCategoryToTransactionsTotalMap } from "~/helpers/transactions";
import { BudgetGroup, getBudgetGroupForCategory } from "~/helpers/budgets";
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

interface BudgetsContentProps {
  budgets: IBudget[];
  categories: ICategory[];
  transactions: ITransaction[];
  selectedDate: Date | null;
  isPending?: boolean;
}

const BudgetsContent = (props: BudgetsContentProps) => {
  const [opened, { open, close }] = useDisclosure(false);
  const [selectedCategory, setSelectedCategory] = React.useState<string | null>(
    null
  );
  const [selectedMonth, setSelectedMonth] = React.useState<Date | null>(null);

  const { t } = useTranslation();

  const categoryToTransactionsTotalMap: Map<string, number> =
    buildCategoryToTransactionsTotalMap(props.transactions);

  const categoryTree = buildCategoriesTree(props.categories);

  const incomeBudgets = props.budgets.filter(
    (budget) =>
      BudgetGroup.Income ===
      getBudgetGroupForCategory(
        getParentCategory(budget.category, props.categories)
      )
  );
  const incomeCategoryTree = categoryTree.filter((category) =>
    areStringsEqual(category.value, "income")
  );
  const expenseBudgets = props.budgets.filter(
    (budget) =>
      BudgetGroup.Spending ===
      getBudgetGroupForCategory(
        getParentCategory(budget.category, props.categories)
      )
  );
  const expenseCategoryTree = categoryTree.filter(
    (category) =>
      !areStringsEqual(category.value, "income") &&
      props.budgets.some((budget) =>
        areStringsEqual(budget.category, category.value)
      )
  );

  const unbudgetedCategoryTree = categoryTree.filter(
    (category) =>
      !props.budgets.some((budget) =>
        areStringsEqual(
          getParentCategory(budget.category, props.categories),
          getParentCategory(category.value, props.categories)
        )
      ) &&
      categoryToTransactionsTotalMap.has(category.value.toLocaleLowerCase())
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
              categories={props.categories}
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
              categories={props.categories}
              selectedDate={props.selectedDate}
              openDetails={openBudgetDetails}
            />
          )}
        </Stack>
        {props.isPending ? (
          <Skeleton h={65} radius="md" />
        ) : (
          <UnbudgetedGroup
            categoryTree={unbudgetedCategoryTree}
            categoryToTransactionsTotalMap={categoryToTransactionsTotalMap}
            categories={props.categories}
            selectedDate={props.selectedDate}
            openDetails={openBudgetDetails}
          />
        )}
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
          transactions={props.transactions}
          categoryToTransactionsTotalMap={categoryToTransactionsTotalMap}
          unbudgetedCategoryTree={unbudgetedCategoryTree}
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
