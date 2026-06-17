import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { useTranslation } from "react-i18next";
import {
  applicationUserQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useDisconnectOidcLoginMutation = () => {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async () =>
      await request({
        url: "/api/applicationUser/disconnectOidcLogin",
        method: "DELETE",
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [applicationUserQueryKey],
      });

      notifications.show({
        color: "var(--button-color-confirm)",
        message: t("oidc_provider_disconnected_successfully"),
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });
};
