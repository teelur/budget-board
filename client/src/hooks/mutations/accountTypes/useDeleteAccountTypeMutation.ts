import { NotificationType, showNotification } from "~/helpers/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  accountsQueryKey,
  accountTypesQueryKey,
  institutionsQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useDeleteAccountTypeMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (accountTypeId: string) =>
      await request({
        url: "/api/accountType",
        method: "DELETE",
        params: { accountTypeId },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [accountTypesQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [institutionsQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [accountsQueryKey] });
    },
    onError: (error: AxiosError) =>
      showNotification({
        type: NotificationType.Error,
        message: translateAxiosError(error),
      }),
  });
};
