import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  recurringForecastQueryKey,
  recurringRulesQueryKey,
  transactionsQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useUnassignRecurringTransactionMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (transactionID: string) =>
      await request({
        url: `/api/recurringRule/transactions/${transactionID}`,
        method: "DELETE",
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [transactionsQueryKey] });
      await queryClient.invalidateQueries({
        queryKey: [recurringRulesQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [recurringForecastQueryKey],
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        message: translateAxiosError(error),
        color: "var(--button-color-destructive)",
      });
    },
  });
};