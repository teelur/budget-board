import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  accountsQueryKey,
  balancesQueryKey,
  institutionsQueryKey,
  transactionsQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useRestoreTransactionMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (transactionIds: string[]) => {
      return await request({
        url: "/api/transaction/restore",
        method: "POST",
        params: { transactionIds },
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [transactionsQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [balancesQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [accountsQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [institutionsQueryKey] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });
};
