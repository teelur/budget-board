import { hasLength, useField } from "@mantine/form";
import { Button, LoadingOverlay, Stack } from "@mantine/core";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { notifications } from "@mantine/notifications";
import { translateAxiosError, ValidationError } from "~/helpers/requests";
import { AxiosError } from "axios";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import PasswordInput from "~/components/core/Input/PasswordInput/PasswordInput";
import { useTranslation } from "react-i18next";

const ResetPassword = (): React.ReactNode => {
  const { t } = useTranslation();

  const oldPasswordField = useField<string>({
    initialValue: "",
    validate: hasLength(
      { min: 3 },
      t("password_min_length_message", { minLength: 3 })
    ),
  });
  const newPasswordField = useField<string>({
    initialValue: "",
    validate: hasLength(
      { min: 3 },
      t("password_min_length_message", { minLength: 3 })
    ),
  });
  const confirmNewPasswordField = useField<string>({
    initialValue: "",
    validate: (value: string) =>
      value !== newPasswordField.getValue()
        ? t("passwords_do_not_match")
        : null,
  });

  type ResetPasswordData = {
    oldPassword: string;
    newPassword: string;
  };

  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doResetPassword = useMutation({
    mutationFn: async (resetPasswordData: ResetPasswordData) =>
      await request({
        url: "/api/manage/info",
        method: "POST",
        data: {
          newPassword: resetPasswordData.newPassword,
          oldPassword: resetPasswordData.oldPassword,
        },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["user"] });

      oldPasswordField.reset();
      newPasswordField.reset();
      confirmNewPasswordField.reset();

      notifications.show({
        color: "var(--button-color-confirm)",
        message: t("password_updated_successfully"),
      });
    },
    onError: (error: AxiosError) => {
      if (error?.response?.data) {
        const errorData = error.response.data as ValidationError;
        if (
          error.status === 400 &&
          errorData.title === "One or more validation errors occurred."
        ) {
          notifications.show({
            title: t("one_or_more_validation_errors_occurred"),
            color: "var(--button-color-destructive)",
            message: Object.values(errorData.errors).join("\n"),
          });
        }
      } else {
        notifications.show({
          color: "var(--button-color-destructive)",
          message: translateAxiosError(error),
        });
      }
    },
  });

  return (
    <Card elevation={1}>
      <LoadingOverlay visible={doResetPassword.isPending} />
      <Stack gap="1rem">
        <PrimaryText size="lg">{t("reset_password")}</PrimaryText>
        <PasswordInput
          {...oldPasswordField.getInputProps()}
          label={<PrimaryText size="sm">{t("current_password")}</PrimaryText>}
          w="100%"
          elevation={1}
        />
        <PasswordInput
          {...newPasswordField.getInputProps()}
          label={<PrimaryText size="sm">{t("new_password")}</PrimaryText>}
          w="100%"
          elevation={1}
        />
        <PasswordInput
          {...confirmNewPasswordField.getInputProps()}
          label={
            <PrimaryText size="sm">{t("confirm_new_password")}</PrimaryText>
          }
          w="100%"
          elevation={1}
        />
        <Button
          onClick={() => {
            oldPasswordField.validate();
            newPasswordField.validate();
            confirmNewPasswordField.validate();

            if (
              !oldPasswordField.error &&
              !newPasswordField.error &&
              !confirmNewPasswordField.error
            ) {
              doResetPassword.mutate({
                oldPassword: oldPasswordField.getValue(),
                newPassword: newPasswordField.getValue(),
              } as ResetPasswordData);
            }
          }}
        >
          {t("reset_password")}
        </Button>
      </Stack>
    </Card>
  );
};

export default ResetPassword;
