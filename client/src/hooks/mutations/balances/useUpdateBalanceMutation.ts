import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  accountsQueryKey,
  balancesQueryKey,
  institutionsQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { IBalanceUpdateRequest } from "~/models/balance";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

interface IUpdateBalanceMutation {
  accountID: string;
}

export const useUpdateBalanceMutation = ({
  accountID,
}: IUpdateBalanceMutation) => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (updatedBalance: IBalanceUpdateRequest) =>
      await request({
        url: `/api/balance`,
        method: "PUT",
        data: updatedBalance,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [balancesQueryKey, accountID],
      });
      await queryClient.invalidateQueries({ queryKey: [accountsQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [institutionsQueryKey] });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });
};
