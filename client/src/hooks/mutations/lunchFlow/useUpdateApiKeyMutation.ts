import { NotificationType, showNotification } from "~/helpers/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  applicationUserQueryKey,
  lunchFlowAccountQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useUpdateApiKeyMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (apiKey: string) =>
      await request({
        url: "/api/lunchFlow/updateApiKey",
        method: "PUT",
        params: { apiKey },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [applicationUserQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [lunchFlowAccountQueryKey],
      });
    },
    onError: (error: AxiosError) => {
      showNotification({
        type: NotificationType.Error,
        message: translateAxiosError(error),
      });
    },
  });
};
