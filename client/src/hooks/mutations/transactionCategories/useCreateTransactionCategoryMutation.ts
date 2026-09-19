import { NotificationType, showNotification } from "~/helpers/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  transactionCategoriesQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { ICategoryCreateRequest } from "~/models/category";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useCreateTransactionCategoryMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (newTransactionCategory: ICategoryCreateRequest) =>
      await request({
        url: "/api/transactionCategory",
        method: "POST",
        data: newTransactionCategory,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [transactionCategoriesQueryKey],
      });
    },
    onError: (error: AxiosError) =>
      showNotification({
        type: NotificationType.Error,
        message: translateAxiosError(error),
      }),
  });
};
