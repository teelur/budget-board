import { Group, Stack } from "@mantine/core";
import { IBudget } from "~/models/budget";
import React from "react";
import { ICategoryNode } from "~/models/category";
import {
  buildCategoryToBudgetsMap,
  buildCategoryToLimitsMap,
} from "~/helpers/budgets";
import BudgetParentCard from "./BudgetParentCard/BudgetParentCard";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { InfoIcon } from "lucide-react";

interface BudgetsGroupProps {
  budgets: IBudget[];
  categoryToTransactionsTotalMap: Map<string, number>;
  categoryToRecurringForecastTotalMap: Map<string, number>;
  categoryTree: ICategoryNode[];
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
    props.categoryTree,
  );

  const toggleCategory = (category: string) => {
    const key = category.toLowerCase();
    setCollapsedCategories((current) => ({
      ...current,
      [key]: !current[key],
    }));
  };

  return (
    <Stack gap="0.75rem" align="center">
      {props.budgets.length > 0 ? (
        props.categoryTree.map((category) => {
          if (
            categoryToBudgetsMap.has(category.value.toLocaleLowerCase()) ||
            category.subCategories.some((subCategory) =>
              categoryToBudgetsMap.has(subCategory.value.toLocaleLowerCase()),
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
                categoryToRecurringForecastTotalMap={
                  props.categoryToRecurringForecastTotalMap
                }
                selectedDate={props.selectedDate}
                openDetails={props.openDetails}
                isCollapsed={
                  collapsedCategories[category.value.toLowerCase()] ?? false
                }
                toggleCollapsed={() => toggleCategory(category.value)}
              />
            );
          }
          return null;
        })
      ) : (
        <Group justify="center" align="center" gap="0.5rem">
          <InfoIcon size={20} color="var(--base-color-text-dimmed)" />
          <DimmedText size="sm">{t("no_budgets")}</DimmedText>
        </Group>
      )}
    </Stack>
  );
};

export default BudgetsGroup;
