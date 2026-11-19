import { NotificationType, showNotification } from "~/helpers/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  accountsQueryKey,
  balancesQueryKey,
  institutionsQueryKey,
  transactionsQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import {
  ITransaction,
  ITransactionTransferCreateRequest,
} from "~/models/transaction";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useCreateTransferMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (
      transfer: ITransactionTransferCreateRequest,
    ): Promise<ITransaction[]> => {
      const response = await request({
        url: "/api/transaction/transfer",
        method: "POST",
        data: transfer,
      });

      return response.data as ITransaction[];
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [transactionsQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [balancesQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [accountsQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [institutionsQueryKey] });
    },
    onError: (error: AxiosError) => {
      showNotification({
        message: translateAxiosError(error),
        type: NotificationType.Error,
      });
    },
  });
};
