import { Group, Stack } from "@mantine/core";
import { BodyText } from "@teelur/budget-board-ui";
import { IBudget } from "~/models/budget";
import React from "react";
import { ICategoryNode } from "~/models/category";
import {
  buildCategoryToBudgetsMap,
  buildCategoryToLimitsMap,
  buildCategoryToRolloverMap,
} from "~/helpers/budgets";
import { useTranslation } from "react-i18next";
import { InfoIcon } from "lucide-react";
import BudgetCard from "./BudgetCard/BudgetCard";

interface BudgetsGroupProps {
  budgets: IBudget[];
  categoryToTransactionsTotalMap: Map<string, number>;
  categoryToRecurringForecastTotalMap: Map<string, number>;
  categoryNodes: ICategoryNode[];
  selectedDate: Date | null;
  openDetails: (category: string, month: Date | null) => void;
}

const BudgetsGroup = (props: BudgetsGroupProps): React.ReactNode => {
  const { t } = useTranslation();
  const [collapsedCategories, setCollapsedCategories] = React.useState<
    Record<string, boolean>
  >({});

  const categoryToBudgetsMap = buildCategoryToBudgetsMap(props.budgets);
  const categoryToLimitsMap = buildCategoryToLimitsMap(
    props.budgets,
    props.categoryNodes,
  );
  const categoryToRolloverMap = buildCategoryToRolloverMap(
    props.budgets,
    props.categoryNodes,
  );

  const toggleCategory = (category: string) => {
    const key = category.toLowerCase();
    setCollapsedCategories((current) => ({
      ...current,
      [key]: !current[key],
    }));
  };

  const getContent = (): React.ReactElement => {
    if (props.budgets.length === 0) {
      return (
        <Group justify="center" align="center" gap="0.5rem">
          <InfoIcon size={20} color="var(--bbui-color-text-muted)" />
          <BodyText tone="muted" size="sm">
            {t("no_budgets")}
          </BodyText>
        </Group>
      );
    }

    const budgetCards = [];
    for (const categoryNode of props.categoryNodes) {
      if (
        categoryToBudgetsMap.has(categoryNode.value.toLocaleLowerCase()) ||
        categoryNode.subCategories.some((subCategory) =>
          categoryToBudgetsMap.has(subCategory.value.toLocaleLowerCase()),
        )
      ) {
        budgetCards.push(
          <BudgetCard
            key={categoryNode.value}
            categoryNode={categoryNode}
            categoryToBudgetsMap={categoryToBudgetsMap}
            categoryToLimitsMap={categoryToLimitsMap}
            categoryToRolloverMap={categoryToRolloverMap}
            categoryToTransactionsTotalMap={
              props.categoryToTransactionsTotalMap
            }
            categoryToRecurringForecastTotalMap={
              props.categoryToRecurringForecastTotalMap
            }
            selectedDate={props.selectedDate}
            openDetails={props.openDetails}
            isCollapsed={
              collapsedCategories[categoryNode.value.toLowerCase()] ?? false
            }
            toggleCollapsed={() => toggleCategory(categoryNode.value)}
          />,
        );
      }
    }
    return <>{budgetCards}</>;
  };

  return (
    <Stack gap="0.75rem" align="center">
      {getContent()}
    </Stack>
  );
};

export default BudgetsGroup;
