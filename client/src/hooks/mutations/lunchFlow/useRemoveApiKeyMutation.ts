import { NotificationType, showNotification } from "~/helpers/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  accountsQueryKey,
  applicationUserQueryKey,
  institutionsQueryKey,
  lunchFlowAccountQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useRemoveApiKeyMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async () =>
      await request({
        url: "/api/lunchFlow/removeApiKey",
        method: "POST",
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [applicationUserQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [lunchFlowAccountQueryKey],
      });
      await queryClient.invalidateQueries({ queryKey: [institutionsQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [accountsQueryKey] });
    },
    onError: (error: AxiosError) => {
      showNotification({
        type: NotificationType.Error,
        message: translateAxiosError(error),
      });
    },
  });
};
