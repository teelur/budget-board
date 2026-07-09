import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  automaticRulesQueryKey,
  budgetsQueryKey,
  transactionCategoriesQueryKey,
  transactionsQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { ICategoryUpdateRequest } from "~/models/category";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useUpdateTransactionCategoryMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (updatedTransactionCategory: ICategoryUpdateRequest) =>
      await request({
        url: "/api/transactionCategory",
        method: "PUT",
        data: updatedTransactionCategory,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [transactionCategoriesQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [transactionsQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [budgetsQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [automaticRulesQueryKey],
      });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });
};
