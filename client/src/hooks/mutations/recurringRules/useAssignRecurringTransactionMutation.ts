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

interface AssignRecurringTransactionInput {
  recurringRuleID: string;
  transactionID: string;
}

export const useAssignRecurringTransactionMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async ({
      recurringRuleID,
      transactionID,
    }: AssignRecurringTransactionInput) =>
      await request({
        url: `/api/recurringRule/${recurringRuleID}/transactions/${transactionID}`,
        method: "POST",
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