import { NotificationType, showNotification } from "~/helpers/notifications";
import { useMutation } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { IOidcCallbackRequest } from "~/models/oidc";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useOidcCallbackMutation = () => {
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (oidcCallbackRequest: IOidcCallbackRequest) =>
      await request({
        url: "/api/oidc/callback",
        method: "POST",
        data: oidcCallbackRequest,
      }),
    onError: (error: AxiosError) =>
      showNotification({
        type: NotificationType.Error,
        message: translateAxiosError(error),
      }),
  });
};
