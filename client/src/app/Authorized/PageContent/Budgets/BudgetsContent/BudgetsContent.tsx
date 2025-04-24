import { Group, Skeleton, Stack } from "@mantine/core";
import { IBudget } from "~/models/budget";
import { getIsParentCategory, getParentCategory } from "~/helpers/category";
import { ICategory } from "~/models/category";
import { ITransaction } from "~/models/transaction";
import { buildCategoryToTransactionsTotalMap } from "~/helpers/transactions";
import {
  BudgetGroup,
  buildBudgetCategoryTree,
  getBudgetGroupForCategory,
} from "~/helpers/budgets";
import BudgetsGroupHeader from "./BudgetGroupHeader/BudgetsGroupHeader";
import BudgetTotalCard from "./BudgetTotalCard/BudgetTotalCard";
import BudgetsGroup from "./BudgetsGroup/BudgetsGroup";
import UnbudgetedGroup from "./UnbudgetedGroup/UnbudgetedGroup";
import { areStringsEqual } from "~/helpers/utils";

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

  const budgetCategoryTree = buildBudgetCategoryTree(
    props.budgets,
    props.categories
  ).sort((a, b) => {
    return a.value.localeCompare(b.value);
  });

  // TODO: Finish filtering out unbudgeted categories
  const unbudgetedCategoryToTransactionsTotalMap = new Map<string, number>(
    Array.from(categoryToTransactionsTotalMap.entries()).filter(
      ([key, _value]) => {
        if (key.length === 0) {
          return true;
        }
        if (getIsParentCategory(key, props.categories)) {
          return !budgetCategoryTree.some((budgetCategory) =>
            areStringsEqual(budgetCategory.value, key)
          );
        }
        const parentCategory = getParentCategory(key, props.categories);
        return !budgetCategoryTree.some(
          (budgetCategory) =>
            areStringsEqual(budgetCategory.value, parentCategory) ||
            budgetCategory.subCategories.some((subCategory) =>
              areStringsEqual(subCategory.value, parentCategory)
            )
        );
      }
    )
  );

  const incomeBudgets = props.budgets.filter(
    (budget) =>
      BudgetGroup.Income ===
      getBudgetGroupForCategory(
        getParentCategory(budget.category, props.categories)
      )
  );
  const incomeCategoryTree = budgetCategoryTree.filter((category) =>
    areStringsEqual(category.value, "income")
  );
  const expenseBudgets = props.budgets.filter(
    (budget) =>
      BudgetGroup.Spending ===
      getBudgetGroupForCategory(
        getParentCategory(budget.category, props.categories)
      )
  );
  const expenseCategoryTree = budgetCategoryTree.filter(
    (category) => !areStringsEqual(category.value, "income")
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
            />
          )}
        </Stack>
        {props.isPending ? (
          <Skeleton h={65} radius="md" />
        ) : (
          <UnbudgetedGroup
            unbudgetedCategoryToTransactionsTotalMap={
              unbudgetedCategoryToTransactionsTotalMap
            }
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
          budgets={props.budgets}
          categories={props.categories}
          transactions={props.transactions}
          isPending={props.isPending ?? false}
        />
      </Stack>
    </Group>
  );
};

export default BudgetsContent;
