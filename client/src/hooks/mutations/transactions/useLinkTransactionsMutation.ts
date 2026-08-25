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
import { ITransaction, ITransactionLinkRequest } from "~/models/transaction";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useLinkTransactionsMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (
      linkRequest: ITransactionLinkRequest,
    ): Promise<ITransaction[]> => {
      const response = await request({
        url: "/api/transaction/link",
        method: "POST",
        data: linkRequest,
      });

      return response.data as ITransaction[];
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
