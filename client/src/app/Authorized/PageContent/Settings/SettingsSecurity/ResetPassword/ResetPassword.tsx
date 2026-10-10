import { hasLength, useField } from "@mantine/form";
import { LoadingOverlay, Stack } from "@mantine/core";
import {
  Button,
  Card,
  HeadingText,
  PasswordInput,
} from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import { useUpdatePasswordMutation } from "~/hooks/mutations/auth/useUpdatePasswordMutation";

const ResetPassword = (): React.ReactNode => {
  const { t } = useTranslation();
  const updatePasswordMutation = useUpdatePasswordMutation();

  const oldPasswordField = useField<string>({
    initialValue: "",
    validate: hasLength(
      { min: 3 },
      t("password_min_length_message", { minLength: 3 }),
    ),
  });
  const newPasswordField = useField<string>({
    initialValue: "",
    validate: hasLength(
      { min: 3 },
      t("password_min_length_message", { minLength: 3 }),
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

  return (
    <Card>
      <Stack gap="1rem">
        <HeadingText tone="heading" level={4}>
          {t("reset_password")}
        </HeadingText>
        <Stack gap="0.5rem">
          <PasswordInput
            {...oldPasswordField.getInputProps()}
            label={t("current_password")}
            w="100%"
          />
          <PasswordInput
            {...newPasswordField.getInputProps()}
            label={t("new_password")}
            w="100%"
          />
          <PasswordInput
            {...confirmNewPasswordField.getInputProps()}
            label={t("confirm_new_password")}
            w="100%"
          />
        </Stack>
        <Button
          variant="filled"
          color="primary"
          size="xs"
          loading={updatePasswordMutation.isPending}
          onClick={() => {
            oldPasswordField.validate();
            newPasswordField.validate();
            confirmNewPasswordField.validate();

            if (
              !oldPasswordField.error &&
              !newPasswordField.error &&
              !confirmNewPasswordField.error
            ) {
              updatePasswordMutation.mutate(
                {
                  oldPassword: oldPasswordField.getValue(),
                  newPassword: newPasswordField.getValue(),
                } as ResetPasswordData,
                {
                  onSuccess: () => {
                    oldPasswordField.reset();
                    newPasswordField.reset();
                    confirmNewPasswordField.reset();
                  },
                },
              );
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
