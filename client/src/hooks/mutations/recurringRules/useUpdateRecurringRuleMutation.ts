import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  recurringForecastQueryKey,
  recurringRulesQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { IRecurringRuleUpdateRequest } from "~/models/recurringRule";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useUpdateRecurringRuleMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (data: IRecurringRuleUpdateRequest) =>
      await request({
        url: "/api/recurringRule",
        method: "PUT",
        data,
      }),
    onSuccess: async () => {
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
