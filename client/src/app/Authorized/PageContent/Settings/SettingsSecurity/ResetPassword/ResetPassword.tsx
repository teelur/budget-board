import { hasLength, useField } from "@mantine/form";
import { Button, LoadingOverlay, Stack } from "@mantine/core";
import React from "react";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import PasswordInput from "~/components/core/Input/PasswordInput/PasswordInput";
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
