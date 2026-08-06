import { notifications } from "@mantine/notifications";
import { useMutation } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { translateAxiosError, ValidationError } from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useTranslation } from "react-i18next";
import { RegisterResponse } from "~/models/auth";

export type RegisterData = {
  email: string;
  password: string;
};

export const useRegisterMutation = () => {
  const { request } = useAuth();
  const { t } = useTranslation();

  return useMutation({
    mutationFn: async (registerData: RegisterData) =>
      await request({
        url: "/api/register",
        method: "POST",
        data: {
          email: registerData.email,
          password: registerData.password,
        },
      }),
    onSuccess: async (res: AxiosResponse) => {
      notifications.show({
        color: "var(--button-color-confirm)",
        message: `${t("account_created_message")}${
          (res.data as RegisterResponse).emailConfirmationRequired
            ? t("account_created_check_your_email_message")
            : ""
        }`,
      });
    },
    onError: (error: AxiosError) => {
      if (
        error?.response?.data &&
        error.response?.status === 400 &&
        (error.response.data as ValidationError).title ===
          "One or more validation errors occurred."
      ) {
        notifications.show({
          title: t("validation_errors_occurred_message"),
          color: "var(--button-color-destructive)",
          message: Object.values(
            (error.response.data as ValidationError).errors,
          ).join("\n"),
        });
      } else {
        notifications.show({
          color: "var(--button-color-destructive)",
          message: translateAxiosError(error),
        });
      }
    },
  });
};
