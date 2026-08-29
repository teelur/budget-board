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

export const useDeleteRecurringRuleMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (recurringRuleID: string) =>
      await request({
        url: "/api/recurringRule",
        method: "DELETE",
        params: { recurringRuleID },
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
