import { Group, Skeleton, Stack } from "@mantine/core";
import { IBudget } from "~/models/budget";
import { buildCategoriesTree, getParentCategory } from "~/helpers/category";
import { ICategory } from "~/models/category";
import { ITransaction } from "~/models/transaction";
import { buildCategoryToTransactionsTotalMap } from "~/helpers/transactions";
import { BudgetGroup, getBudgetGroupForCategory } from "~/helpers/budgets";
import BudgetsGroupHeader from "./BudgetGroupHeader/BudgetsGroupHeader";
import BudgetTotalCard from "./BudgetTotalCard/BudgetTotalCard";
import BudgetsGroup from "./BudgetsGroup/BudgetsGroup";
import UnbudgetedGroup from "./UnbudgetedGroup/UnbudgetedGroup";
import { areStringsEqual } from "~/helpers/utils";
import FixParentBudgetButton from "./FixParentBudgetButton/FixParentBudgetButton";

interface BudgetsContentProps {
  budgets: IBudget[];
  categories: ICategory[];
  transactions: ITransaction[];
  selectedDate?: Date;
  isPending?: boolean;
}

const BudgetsContent = (props: BudgetsContentProps) => {
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

  return (
    <Group gap="0.5rem" align="flex-start">
      <Stack w={{ base: "100%", md: "70%" }}>
        <Stack gap="0.5rem">
          <BudgetsGroupHeader groupName="Income" />
          {props.isPending ? (
            <Skeleton h={65} radius="md" />
          ) : (
            <BudgetsGroup
              budgets={incomeBudgets}
              categoryTree={incomeCategoryTree}
              categoryToTransactionsTotalMap={categoryToTransactionsTotalMap}
              categories={props.categories}
              selectedDate={props.selectedDate}
            />
          )}
        </Stack>
        <Stack gap="0.5rem">
          <BudgetsGroupHeader groupName="Expenses" />
          {props.isPending ? (
            <Skeleton h={65} radius="md" />
          ) : (
            <BudgetsGroup
              budgets={expenseBudgets}
              categoryTree={expenseCategoryTree}
              categoryToTransactionsTotalMap={categoryToTransactionsTotalMap}
              categories={props.categories}
              selectedDate={props.selectedDate}
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
          />
        )}
      </Stack>
      <Stack
        style={{ flexGrow: 1 }}
        w={{ base: "100%", md: "20%" }}
        h={{ base: "auto", md: "100%" }}
      >
        <BudgetTotalCard
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
