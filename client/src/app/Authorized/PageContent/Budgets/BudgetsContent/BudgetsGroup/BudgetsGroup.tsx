import classes from "./BudgetsGroup.module.css";

import { Group, Stack, Text } from "@mantine/core";
import { IBudget } from "~/models/budget";
import React from "react";
import BudgetCard from "./BudgetCard/BudgetCard";
import { getParentCategory } from "~/helpers/category";
import { ICategory, ICategoryNode } from "~/models/category";
import {
  BudgetGroup,
  getBudgetAmount,
  getBudgetGroupForCategory,
  getTotalLimitForCategory,
  groupBudgetsByCategory,
} from "~/helpers/budgets";
import { CornerDownRightIcon } from "lucide-react";

interface BudgetsGroupProps {
  budgets: IBudget[];
  categoryToTransactionsTotalMap: Map<string, number>;
  categoryTree: ICategoryNode[];
  categories: ICategory[];
}

const BudgetsGroup = (props: BudgetsGroupProps): React.ReactNode => {
  const categoryToBudgetsMap = groupBudgetsByCategory(props.budgets);

  // const buildCardsList = (): React.ReactNode[] => {
  //   const cards: React.ReactNode[] = [];
  //   categoryToBudgetsMap.forEach((budgets, category) =>
  //     cards.push(
  //       <BudgetCard2
  //         key={category}
  //         budgets={budgets}
  //         categoryDisplayString={getFormattedCategoryValue(
  //           category,
  //           props.categories
  //         )}
  //         amount={getBudgetAmount(
  //           category.toLocaleLowerCase(),
  //           props.categoryToTransactionsTotalMap,
  //           props.categories
  //         )}
  //         isIncome={
  //           BudgetGroup.Income ===
  //           getBudgetGroupForCategory(
  //             getParentCategory(category, props.categories)
  //           )
  //         }
  //       />
  //     )
  //   );
  //   return cards;
  // };

  const buildBudgetsCardsList = (
    categoryTree: ICategoryNode[]
  ): React.ReactNode[] => {
    const cards: React.ReactNode[] = [];

    categoryTree.forEach((category) => {
      cards.push(
        <BudgetCard
          key={category.value}
          id={
            (categoryToBudgetsMap.get(category.value)?.length ?? 0) === 1
              ? categoryToBudgetsMap.get(category.value)!.at(0)!.id
              : ""
          }
          categoryDisplayString={category.value}
          amount={
            props.categoryToTransactionsTotalMap.get(
              category.value.toLocaleLowerCase()
            ) ?? 0
          }
          limit={getTotalLimitForCategory(props.budgets, category)}
          isIncome={
            BudgetGroup.Income ===
            getBudgetGroupForCategory(
              getParentCategory(category.value, props.categories)
            )
          }
        />
      );

      category.subCategories
        .sort((a, b) => a.value.localeCompare(b.value))
        .forEach((subCategory) => {
          cards.push(
            <Group gap={10} key={subCategory.value} w="100%" wrap="nowrap">
              <CornerDownRightIcon />
              <BudgetCard
                id=""
                categoryDisplayString={subCategory.value}
                amount={getBudgetAmount(
                  subCategory.value.toLocaleLowerCase(),
                  props.categoryToTransactionsTotalMap,
                  props.categories
                )}
                limit={getTotalLimitForCategory(props.budgets, subCategory)}
                isIncome={
                  BudgetGroup.Income ===
                  getBudgetGroupForCategory(
                    getParentCategory(category.value, props.categories)
                  )
                }
              />
            </Group>
          );
        });
    });

    return cards;
  };

  return (
    <Stack className={classes.root}>
      {props.budgets.length > 0 ? (
        <>
          <>{buildBudgetsCardsList(props.categoryTree)}</>
          {/* <>{buildCardsList()}</> */}
        </>
      ) : (
        <Text size="sm">No budgets.</Text>
      )}
    </Stack>
  );
};

export default BudgetsGroup;
