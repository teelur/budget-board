import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { accountsQueryKey, translateAxiosError } from "~/helpers/requests";
import { IAccountIndexRequest } from "~/models/account";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useOrderAccountsMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (orderedAccounts: IAccountIndexRequest[]) =>
      await request({
        url: "/api/account/order",
        method: "PUT",
        data: orderedAccounts,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [accountsQueryKey] });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });
};
