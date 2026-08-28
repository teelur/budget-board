import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  recurringForecastQueryKey,
  recurringRulesQueryKey,
  transactionsQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { IRecurringRuleCreateRequest } from "~/models/recurringRule";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

interface CreateRecurringRuleMutationInput {
  data: IRecurringRuleCreateRequest;
  transactionID?: string;
}

export const useCreateRecurringRuleMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async ({
      data,
      transactionID,
    }: CreateRecurringRuleMutationInput) =>
      await request({
        url: transactionID
          ? `/api/recurringRule/from-transaction/${transactionID}`
          : "/api/recurringRule",
        method: "POST",
        data,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [recurringRulesQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [recurringForecastQueryKey],
      });
      await queryClient.invalidateQueries({ queryKey: [transactionsQueryKey] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        message: translateAxiosError(error),
        color: "var(--button-color-destructive)",
      });
    },
  });
};
