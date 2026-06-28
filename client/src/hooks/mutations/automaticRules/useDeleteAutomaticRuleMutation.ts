import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  automaticRulesQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useDeleteAutomaticRuleMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (automaticRuleId: string) => {
      await request({
        url: `/api/automaticRule`,
        method: "DELETE",
        params: { automaticRuleId },
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: [automaticRulesQueryKey],
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
