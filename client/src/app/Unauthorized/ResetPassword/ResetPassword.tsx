import { Stack, Button, LoadingOverlay } from "@mantine/core";
import { hasLength, useField } from "@mantine/form";
import React from "react";
import { LoginCardState } from "../Welcome";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import PasswordInput from "~/components/core/Input/PasswordInput/PasswordInput";
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
      <LoadingOverlay
        visible={resetPasswordMutation.isPending}
        zIndex={1000}
        overlayProps={{ radius: "sm", blur: 2 }}
      />
      <Stack align="center" gap="0.5rem" w="100%">
        <TextInput
          label={<PrimaryText size="sm">{t("reset_code")}</PrimaryText>}
          w="100%"
          {...resetCodeField.getInputProps()}
          elevation={1}
        />
        <PasswordInput
          label={<PrimaryText size="sm">{t("new_password")}</PrimaryText>}
          w="100%"
          {...passwordField.getInputProps()}
          elevation={1}
        />
        <PasswordInput
          label={<PrimaryText size="sm">{t("confirm_password")}</PrimaryText>}
          w="100%"
          {...confirmPasswordField.getInputProps()}
          elevation={1}
        />
        <Button
          variant="filled"
          fullWidth
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
      </Stack>
      <Button
        variant="default"
        fullWidth
        onClick={() => props.setLoginCardState(LoginCardState.Login)}
      >
        {t("return_to_login")}
      </Button>
    </Stack>
  );
};

export default ResetPassword;
