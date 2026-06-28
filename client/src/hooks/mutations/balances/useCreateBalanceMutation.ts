import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  accountsQueryKey,
  balancesQueryKey,
  institutionsQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { IBalanceCreateRequest } from "~/models/balance";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

interface ICreateBalanceMutation {
  accountId: string;
}

export const useCreateBalanceMutation = ({
  accountId,
}: ICreateBalanceMutation) => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (newBalance: IBalanceCreateRequest) =>
      await request({
        url: "/api/balance",
        method: "POST",
        data: newBalance,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [institutionsQueryKey] });
      await queryClient.invalidateQueries({
        queryKey: [accountsQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [balancesQueryKey, accountId],
      });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });
};
