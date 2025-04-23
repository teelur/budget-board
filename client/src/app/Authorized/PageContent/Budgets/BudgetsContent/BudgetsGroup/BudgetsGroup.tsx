import classes from "./BudgetsGroup.module.css";

import { Stack, Text } from "@mantine/core";
import { IBudget, IBudgetUpdateRequest } from "~/models/budget";
import React from "react";
import { ICategory, ICategoryNode } from "~/models/category";
import {
  buildCategoryToBudgetsMap,
  buildCategoryToLimitsMap,
} from "~/helpers/budgets";
import BudgetParentCard from "./BudgetParentCard/BudgetParentCard";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { notifications } from "@mantine/notifications";
import { useQueryClient, useMutation } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";

interface BudgetsGroupProps {
  budgets: IBudget[];
  categoryToTransactionsTotalMap: Map<string, number>;
  categoryTree: ICategoryNode[];
  categories: ICategory[];
}

const BudgetsGroup = (props: BudgetsGroupProps): React.ReactNode => {
  const categoryToBudgetsMap = buildCategoryToBudgetsMap(props.budgets);
  const categoryToLimitsMap = buildCategoryToLimitsMap(
    props.budgets,
    props.categoryTree
  );

  const { request } = React.useContext<any>(AuthContext);
  const queryClient = useQueryClient();
  const doEditBudget = useMutation({
    mutationFn: async (newBudget: IBudgetUpdateRequest) =>
      await request({
        url: "/api/budget",
        method: "PUT",
        data: newBudget,
      }),
    onMutate: async (variables: IBudgetUpdateRequest) => {
      await queryClient.cancelQueries({ queryKey: ["budgets"] });

      const previousBudgets: IBudget[] =
        queryClient.getQueryData(["budgets"]) ?? [];

      queryClient.setQueryData(["budgets"], (oldBudgets: IBudget[]) =>
        oldBudgets?.map((oldBudget) =>
          oldBudget.id === variables.id
            ? { ...oldBudget, limit: variables.limit }
            : oldBudget
        )
      );

      return { previousBudgets };
    },
    onError: (error: AxiosError, _variables: IBudgetUpdateRequest, context) => {
      queryClient.setQueryData(["budgets"], context?.previousBudgets ?? []);
      notifications.show({ message: translateAxiosError(error), color: "red" });
    },
    onSettled: () => queryClient.invalidateQueries({ queryKey: ["budgets"] }),
  });

  const doDeleteBudget = useMutation({
    mutationFn: async (id: string) =>
      await request({
        url: "/api/budget",
        method: "DELETE",
        params: { guid: id },
      }),
    onSuccess: async () =>
      await queryClient.invalidateQueries({ queryKey: ["budgets"] }),
  });

  return (
    <Stack className={classes.root}>
      {props.budgets.length > 0 ? (
        props.categoryTree.map((category) => (
          <BudgetParentCard
            key={category.value}
            categoryTree={category}
            categoryToBudgetsMap={categoryToBudgetsMap}
            categoryToLimitsMap={categoryToLimitsMap}
            categoryToTransactionsTotalMap={
              props.categoryToTransactionsTotalMap
            }
            doEditBudget={doEditBudget.mutate}
            doDeleteBudget={doDeleteBudget.mutate}
            isPending={doEditBudget.isPending || doDeleteBudget.isPending}
          />
        ))
      ) : (
        <Text size="sm">No budgets.</Text>
      )}
    </Stack>
  );
};

export default BudgetsGroup;
