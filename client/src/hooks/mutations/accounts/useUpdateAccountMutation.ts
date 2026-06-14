import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  accountsQueryKey,
  institutionsQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { IAccountUpdateRequest } from "~/models/account";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useUpdateAccountMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (updatedAccount: IAccountUpdateRequest) =>
      await request({
        url: "/api/account",
        method: "PUT",
        data: updatedAccount,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [accountsQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [institutionsQueryKey] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        message: translateAxiosError(error),
        color: "var(--button-color-destructive)",
      });
    },
  });
};
