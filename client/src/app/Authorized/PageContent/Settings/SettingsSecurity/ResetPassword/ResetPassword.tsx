import { hasLength, useField } from "@mantine/form";
import { LoadingOverlay, Stack } from "@mantine/core";
import { Button, PasswordInput } from "@teelur/budget-board-ui";
import React from "react";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
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
    <Card elevation={1}>
      <LoadingOverlay visible={updatePasswordMutation.isPending} />
      <Stack gap="1rem">
        <PrimaryText size="lg">{t("reset_password")}</PrimaryText>
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
        <Button
          variant="filled"
          color="primary"
          size="xs"
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
