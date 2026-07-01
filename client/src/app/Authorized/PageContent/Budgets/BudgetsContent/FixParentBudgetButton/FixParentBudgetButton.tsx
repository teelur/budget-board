import { Button } from "@mantine/core";
import { areStringsEqual } from "~/helpers/utils";
import { IBudget, IBudgetUpdateRequest } from "~/models/budget";
import { ICategoryNode } from "~/models/category";
import { useTranslation } from "react-i18next";
import { useUpdateBudgetMutation } from "~/hooks/mutations/budgets/useUpdateBudgetMutation";

interface FixParentBudgetButtonProps {
  budgets: IBudget[];
  categoryTree: ICategoryNode[];
  categoryToTransactionsTotalMap: Map<string, number>;
}

const FixParentBudgetButton = (props: FixParentBudgetButtonProps) => {
  const { t } = useTranslation();
  const updateBudgetMutation = useUpdateBudgetMutation();

  const budgetedCategories = props.budgets.map((budget) =>
    budget.category.toLocaleLowerCase(),
  );

  const unbudgetedCategories = props.categoryTree.filter(
    (category) =>
      !budgetedCategories.some((budgetedCategory) =>
        areStringsEqual(budgetedCategory, category.value.toLocaleLowerCase()),
      ) &&
      props.categoryToTransactionsTotalMap.has(
        category.value.toLocaleLowerCase(),
      ),
  );

  const unbudgetedParentCategoriesWithBudgetedChildren =
    unbudgetedCategories.filter((category) =>
      category.subCategories?.some((child) =>
        budgetedCategories.some((budgetedCategory) =>
          areStringsEqual(budgetedCategory, child.value.toLocaleLowerCase()),
        ),
      ),
    );

  const generateParentCateogories = () => {
    unbudgetedParentCategoriesWithBudgetedChildren.forEach((category) => {
      const childBudget = props.budgets.find((budget) =>
        category.subCategories?.some((child) =>
          areStringsEqual(budget.category, child.value),
        ),
      );
      // Just touch the budget to trigger the parent category to be created.
      if (!childBudget) {
        return;
      }
      updateBudgetMutation.mutate({
        id: childBudget.id,
        limit: childBudget.limit,
      } as IBudgetUpdateRequest);
    });
  };

  if (
    unbudgetedParentCategoriesWithBudgetedChildren.length === 0 ||
    updateBudgetMutation.isPending
  ) {
    return null;
  }

  return (
    <Button size="compact-sm" onClick={generateParentCateogories}>
      {t("fix_parent_budgets")}
    </Button>
  );
};
export default FixParentBudgetButton;
