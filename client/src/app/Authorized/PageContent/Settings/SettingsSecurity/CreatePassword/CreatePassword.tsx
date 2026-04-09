import { Button, Stack } from "@mantine/core";
import { hasLength, useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError, ValidationError } from "~/helpers/requests";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import PasswordInput from "~/components/core/Input/PasswordInput/PasswordInput";
import { useTranslation } from "react-i18next";

const CreatePassword = (): React.ReactNode => {
  const { t } = useTranslation();

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

  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doCreatePassword = useMutation({
    mutationFn: async () =>
      await request({
        url: "/api/manage/info",
        method: "POST",
        data: {
          newPassword: newPasswordField.getValue(),
        },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["user"] });
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
      <Stack gap="1rem">
        <PrimaryText size="lg">{t("create_password")}</PrimaryText>
        <PasswordInput
          {...newPasswordField.getInputProps()}
          label={<PrimaryText size="sm">{t("new_password")}</PrimaryText>}
          w="100%"
        />
        <PasswordInput
          {...confirmNewPasswordField.getInputProps()}
          label={<PrimaryText size="sm">{t("confirm_password")}</PrimaryText>}
          w="100%"
        />
        <Button
          onClick={() => {
            newPasswordField.validate();
            confirmNewPasswordField.validate();

            doCreatePassword.mutate();
          }}
          loading={doCreatePassword.isPending}
        >
          {t("create_password")}
        </Button>
      </Stack>
    </Card>
  );
};

export default CreatePassword;
