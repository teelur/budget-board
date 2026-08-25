import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  accountsQueryKey,
  balancesQueryKey,
  institutionsQueryKey,
  transactionLinkCandidatesQueryKey,
  transactionsQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useUnlinkTransactionMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (transactionID: string) => {
      await request({
        url: `/api/transaction/unlink/${transactionID}`,
        method: "POST",
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [transactionsQueryKey] });
      await queryClient.invalidateQueries({
        queryKey: [transactionLinkCandidatesQueryKey],
      });
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
