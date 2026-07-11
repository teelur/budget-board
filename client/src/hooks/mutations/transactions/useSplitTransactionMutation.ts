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
import { ITransactionSplitRequest } from "~/models/transaction";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useSplitTransactionMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (splitTransaction: ITransactionSplitRequest) =>
      await request({
        url: "/api/transaction/split",
        method: "POST",
        data: splitTransaction,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [transactionsQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [balancesQueryKey] });
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
