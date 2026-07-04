import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  applicationUserQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { IOidcCallbackRequest } from "~/models/oidc";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useOidcCallbackMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (oidcCallbackRequest: IOidcCallbackRequest) =>
      await request({
        url: "/api/oidc/callback",
        method: "POST",
        data: oidcCallbackRequest,
      }),
    onSuccess: async () => {
      queryClient.invalidateQueries({ queryKey: [applicationUserQueryKey] });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });
};
