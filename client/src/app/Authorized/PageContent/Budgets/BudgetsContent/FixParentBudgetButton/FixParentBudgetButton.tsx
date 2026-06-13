import { Button } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useQueryClient, useMutation } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError , budgetsQueryKey} from "~/helpers/requests";
import { areStringsEqual } from "~/helpers/utils";
import { IBudget, IBudgetUpdateRequest } from "~/models/budget";
import { ICategoryNode } from "~/models/category";
import { useTranslation } from "react-i18next";

interface FixParentBudgetButtonProps {
  budgets: IBudget[];
  categoryTree: ICategoryNode[];
  categoryToTransactionsTotalMap: Map<string, number>;
}

const FixParentBudgetButton = (props: FixParentBudgetButtonProps) => {
  const { t } = useTranslation();
  const { request } = useAuth();
  const queryClient = useQueryClient();
  const doEditBudget = useMutation({
    mutationFn: async (newBudget: IBudgetUpdateRequest) =>
      await request({
        url: "/api/budget",
        method: "PUT",
        data: newBudget,
      }),
    onMutate: async (variables: IBudgetUpdateRequest) => {
      await queryClient.cancelQueries({ queryKey: [budgetsQueryKey] });

      const previousBudgets: IBudget[] =
        queryClient.getQueryData([budgetsQueryKey]) ?? [];

      queryClient.setQueryData([budgetsQueryKey], (oldBudgets: IBudget[]) =>
        oldBudgets?.map((oldBudget) =>
          oldBudget.id === variables.id
            ? { ...oldBudget, limit: variables.limit }
            : oldBudget
        )
      );

      return { previousBudgets };
    },
    onError: (error: AxiosError, _variables: IBudgetUpdateRequest, context) => {
      queryClient.setQueryData([budgetsQueryKey], context?.previousBudgets ?? []);
      notifications.show({
        message: translateAxiosError(error),
        color: "var(--button-color-destructive)",
      });
    },
    onSettled: () => queryClient.invalidateQueries({ queryKey: [budgetsQueryKey] }),
  });

  const budgetedCategories = props.budgets.map((budget) =>
    budget.category.toLocaleLowerCase()
  );

  const unbudgetedCategories = props.categoryTree.filter(
    (category) =>
      !budgetedCategories.some((budgetedCategory) =>
        areStringsEqual(budgetedCategory, category.value.toLocaleLowerCase())
      ) &&
      props.categoryToTransactionsTotalMap.has(
        category.value.toLocaleLowerCase()
      )
  );

  const unbudgetedParentCategoriesWithBudgetedChildren =
    unbudgetedCategories.filter((category) =>
      category.subCategories?.some((child) =>
        budgetedCategories.some((budgetedCategory) =>
          areStringsEqual(budgetedCategory, child.value.toLocaleLowerCase())
        )
      )
    );

  const generateParentCateogories = () => {
    unbudgetedParentCategoriesWithBudgetedChildren.forEach((category) => {
      const childBudget = props.budgets.find((budget) =>
        category.subCategories?.some((child) =>
          areStringsEqual(budget.category, child.value)
        )
      );
      // Just touch the budget to trigger the parent category to be created.
      if (!childBudget) {
        return;
      }
      doEditBudget.mutate({
        id: childBudget.id,
        limit: childBudget.limit,
      } as IBudgetUpdateRequest);
    });
  };

  if (
    unbudgetedParentCategoriesWithBudgetedChildren.length === 0 ||
    doEditBudget.isPending
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
