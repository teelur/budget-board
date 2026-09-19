import { NotificationType, showNotification } from "~/helpers/notifications";
import { useMutation } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { useTranslation } from "react-i18next";
import { translateAxiosError } from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useForgotPasswordMutation = () => {
  const { request } = useAuth();
  const { t } = useTranslation();

  return useMutation({
    mutationFn: async (email: string) =>
      await request({
        url: "/api/forgotPassword",
        method: "POST",
        data: {
          email,
        },
      }),
    onSuccess: async () => {
      showNotification({
        type: NotificationType.Success,
        message: t("reset_password_request_message"),
      });
    },
    onError: (error: AxiosError) =>
      showNotification({
        type: NotificationType.Error,
        message: translateAxiosError(error),
      }),
  });
};
