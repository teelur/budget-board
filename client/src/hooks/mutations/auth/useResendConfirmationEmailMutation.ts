import { NotificationType, showNotification } from "~/helpers/notifications";
import { useMutation } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useTranslation } from "react-i18next";

export const useResendConfirmationEmailMutation = () => {
  const { request } = useAuth();
  const { t } = useTranslation();

  return useMutation({
    mutationFn: async (email: string) =>
      await request({
        url: "/api/resendConfirmationEmail",
        method: "POST",
        data: {
          email,
        },
      }),
    onSuccess: async () => {
      showNotification({
        type: NotificationType.Success,
        message: t("verification_email_resent_message"),
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
