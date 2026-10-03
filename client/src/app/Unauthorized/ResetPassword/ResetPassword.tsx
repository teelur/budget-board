import { Stack, Group } from "@mantine/core";
import { Button, PasswordInput, TextInput } from "@teelur/budget-board-ui";
import { hasLength, useField } from "@mantine/form";
import React from "react";
import { LoginCardState } from "../Welcome";
import { useTranslation } from "react-i18next";
import { useResetPasswordMutation } from "~/hooks/mutations/auth/useResetPasswordMutation";

interface ResetPasswordProps {
  setLoginCardState: React.Dispatch<React.SetStateAction<LoginCardState>>;
  email: string;
}

const ResetPassword = (props: ResetPasswordProps): React.ReactNode => {
  const { t } = useTranslation();
  const resetPasswordMutation = useResetPasswordMutation();

  const resetCodeField = useField<string>({
    initialValue: "",
    validate: (value) =>
      value.trim() === "" ? t("reset_code_required_message") : null,
  });
  const passwordMinLength = 3;
  const passwordField = useField<string>({
    initialValue: "",
    validate: hasLength(
      { min: passwordMinLength },
      t("password_min_length_message", {
        minLength: passwordMinLength,
      }),
    ),
  });
  const confirmPasswordField = useField<string>({
    initialValue: "",
    validate: (value) =>
      value !== passwordField.getValue() ? t("passwords_do_not_match") : null,
  });

  return (
    <Stack gap="0.75rem" align="center" p="1rem">
      <Stack align="center" gap="0.5rem" w="100%">
        <TextInput
          label={t("reset_code")}
          w="100%"
          {...resetCodeField.getInputProps()}
        />
        <PasswordInput
          label={t("new_password")}
          w="100%"
          {...passwordField.getInputProps()}
        />
        <PasswordInput
          label={t("confirm_password")}
          w="100%"
          {...confirmPasswordField.getInputProps()}
        />
        <Group wrap="nowrap" gap="0.5rem" w="100%">
          <Button
            variant="filled"
            color="neutral"
            size="compact-sm"
            flex="1 1 0"
            onClick={() => props.setLoginCardState(LoginCardState.Login)}
          >
            {t("return_to_login")}
          </Button>
          <Button
            variant="filled"
            color="primary"
            size="compact-sm"
            flex="1 1 0"
            loading={resetPasswordMutation.isPending}
            onClick={() => {
              resetCodeField.validate();
              passwordField.validate();
              confirmPasswordField.validate();

              if (
                resetCodeField.error ||
                passwordField.error ||
                confirmPasswordField.error
              ) {
                return;
              }

              resetPasswordMutation.mutate(
                {
                  email: props.email,
                  resetCode: resetCodeField.getValue(),
                  newPassword: passwordField.getValue(),
                },
                {
                  onSuccess: () => {
                    props.setLoginCardState(LoginCardState.Login);
                  },
                },
              );
            }}
          >
            {t("reset_password")}
          </Button>
        </Group>
      </Stack>
    </Stack>
  );
};

export default ResetPassword;
