import classes from "./BudgetsContent.module.css";

import { Group, Skeleton, Stack } from "@mantine/core";
import { IBudget } from "~/models/budget";
import { getParentCategory, getSubCategories } from "~/helpers/category";
import { ICategory } from "~/models/category";
import { ITransaction } from "~/models/transaction";
import { buildCategoryToTransactionsTotalMap } from "~/helpers/transactions";
import { BudgetGroup, getBudgetGroupForCategory } from "~/helpers/budgets";
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

  const unbudgetedCategoryToTransactionsTotalMap = new Map<string, number>(
    Array.from(categoryToTransactionsTotalMap).filter(
      ([category, _]) =>
        !props.budgets.some(
          (budget) =>
            areStringsEqual(budget.category, category) ||
            getSubCategories(budget.category, props.categories).some(
              (subCategory) => areStringsEqual(subCategory.value, category)
            )
        )
    )
  );

  return (
    <Group className={classes.root}>
      <Stack w={{ base: "100%", md: "70%" }}>
        <Stack className={classes.groupContainer}>
          <BudgetsGroupHeader groupName="Income" />
          {props.isPending ? (
            <Skeleton h={65} radius="md" />
          ) : (
            <BudgetsGroup
              budgets={props.budgets.filter(
                (budget) =>
                  BudgetGroup.Income ===
                  getBudgetGroupForCategory(
                    getParentCategory(budget.category, props.categories)
                  )
              )}
              categoryToTransactionsTotalMap={categoryToTransactionsTotalMap}
              categories={props.categories}
            />
          )}
        </Stack>
        <Stack className={classes.groupContainer}>
          <BudgetsGroupHeader groupName="Expenses" />
          {props.isPending ? (
            <Skeleton h={65} radius="md" />
          ) : (
            <BudgetsGroup
              budgets={props.budgets.filter(
                (budget) =>
                  BudgetGroup.Spending ===
                  getBudgetGroupForCategory(
                    getParentCategory(budget.category, props.categories)
                  )
              )}
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
