import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  transactionCategoriesQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { ICategoryIconUpdateRequest } from "~/models/category";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useSetTransactionCategoryIconMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (
      updatedTransactionCategoryIcon: ICategoryIconUpdateRequest,
    ) =>
      await request({
        url: "/api/transactionCategory/icon",
        method: "PUT",
        data: updatedTransactionCategoryIcon,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [transactionCategoriesQueryKey],
      });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });
};
