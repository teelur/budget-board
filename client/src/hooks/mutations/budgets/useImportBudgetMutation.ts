import { NotificationType, showNotification } from "~/helpers/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  budgetsQueryKey,
  transactionCategoriesQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { IBudgetImportRequest } from "~/models/budgetTemplate";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useImportBudgetMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (importRequest: IBudgetImportRequest) =>
      await request({
        url: "/api/budget/import",
        method: "POST",
        data: importRequest,
      }),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: [budgetsQueryKey] }),
        queryClient.invalidateQueries({
          queryKey: [transactionCategoriesQueryKey],
        }),
      ]);
    },
    onError: (error: AxiosError) => {
      showNotification({
        message: translateAxiosError(error),
        type: NotificationType.Error,
      });
    },
  });
};
