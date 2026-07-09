import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  budgetsQueryKey,
  transactionCategoriesQueryKey,
  transactionsQueryKey,
  translateAxiosError,
  automaticRulesQueryKey,
} from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useDeleteTransactionCategoryMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (guid: string) =>
      await request({
        url: "/api/transactionCategory",
        method: "DELETE",
        params: { guid },
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
