import { notifications } from "@mantine/notifications";
import { useMutation } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { useTranslation } from "react-i18next";
import { translateAxiosError } from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export type ResetPasswordData = {
  email: string;
  resetCode: string;
  newPassword: string;
};

export const useResetPasswordMutation = () => {
  const { request } = useAuth();
  const { t } = useTranslation();

  return useMutation({
    mutationFn: async (resetPasswordData: ResetPasswordData) =>
      await request({
        url: "/api/resetPassword",
        method: "POST",
        data: {
          email: resetPasswordData.email,
          resetCode: resetPasswordData.resetCode,
          newPassword: resetPasswordData.newPassword,
        },
      }),
    onSuccess: async () => {
      notifications.show({
        color: "var(--button-color-confirm)",
        message: t("reset_password_success_message"),
      });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });
};
