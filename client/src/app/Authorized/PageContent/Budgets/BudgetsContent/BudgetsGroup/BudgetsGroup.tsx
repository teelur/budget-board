import classes from "./BudgetsGroup.module.css";

import { Stack, Text } from "@mantine/core";
import { IBudget } from "~/models/budget";
import React from "react";
import { ICategory, ICategoryNode } from "~/models/category";
import {
  buildCategoryToBudgetsMap,
  buildCategoryToLimitsMap,
} from "~/helpers/budgets";
import BudgetParentCard from "./BudgetParentCard/BudgetParentCard";

interface BudgetsGroupProps {
  budgets: IBudget[];
  categoryToTransactionsTotalMap: Map<string, number>;
  categoryTree: ICategoryNode[];
  categories: ICategory[];
  selectedDate?: Date;
  openDetails: (category: string, month: Date) => void;
}

const BudgetsGroup = (props: BudgetsGroupProps): React.ReactNode => {
  const categoryToBudgetsMap = buildCategoryToBudgetsMap(props.budgets);
  const categoryToLimitsMap = buildCategoryToLimitsMap(
    props.budgets,
    props.categoryTree
  );

  return (
    <Stack className={classes.root}>
      {props.budgets.length > 0 ? (
        props.categoryTree.map((category) => {
          if (
            categoryToBudgetsMap.has(category.value.toLocaleLowerCase()) ||
            category.subCategories.some((subCategory) =>
              categoryToBudgetsMap.has(subCategory.value.toLocaleLowerCase())
            )
          ) {
            return (
              <BudgetParentCard
                key={category.value}
                categoryTree={category}
                categoryToBudgetsMap={categoryToBudgetsMap}
                categoryToLimitsMap={categoryToLimitsMap}
                categoryToTransactionsTotalMap={
                  props.categoryToTransactionsTotalMap
                }
                selectedDate={props.selectedDate}
                openDetails={props.openDetails}
              />
            );
          }
          return null;
        })
      ) : (
        <Text size="sm">No budgets.</Text>
      )}
    </Stack>
  );
};

export default BudgetsGroup;
