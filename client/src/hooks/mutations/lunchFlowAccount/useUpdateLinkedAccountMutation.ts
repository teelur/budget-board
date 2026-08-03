import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  accountsQueryKey,
  institutionsQueryKey,
  lunchFlowAccountQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useUpdateLinkedAccountMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (updateLinkedAccountRequest: {
      lunchFlowAccountGuid: string;
      linkedAccountGuid: string | null;
    }) =>
      await request({
        url: "/api/lunchFlowAccount/updateLinkedAccount",
        method: "PUT",
        params: {
          lunchFlowAccountGuid: updateLinkedAccountRequest.lunchFlowAccountGuid,
          linkedAccountGuid: updateLinkedAccountRequest.linkedAccountGuid,
        },
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [lunchFlowAccountQueryKey] });
      queryClient.invalidateQueries({ queryKey: [institutionsQueryKey] });
      queryClient.invalidateQueries({ queryKey: [accountsQueryKey] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });
};
