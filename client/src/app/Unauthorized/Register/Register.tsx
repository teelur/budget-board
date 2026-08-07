import { Stack, Button, LoadingOverlay } from "@mantine/core";
import { hasLength, isEmail, useField } from "@mantine/form";
import React from "react";
import { LoginCardState } from "../Welcome";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import PasswordInput from "~/components/core/Input/PasswordInput/PasswordInput";
import { useTranslation } from "react-i18next";
import { useRegisterMutation } from "~/hooks/mutations/auth/useRegisterMutation";

interface RegisterProps {
  setLoginCardState: React.Dispatch<React.SetStateAction<LoginCardState>>;
}

const Register = (props: RegisterProps): React.ReactNode => {
  const { t } = useTranslation();
  const registerMutation = useRegisterMutation();

  const emailField = useField<string>({
    initialValue: "",
    validate: isEmail(t("invalid_email_message")),
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
        visible={registerMutation.isPending}
        zIndex={1000}
        overlayProps={{ blur: 2 }}
      />
      <Stack align="center" gap="0.5rem" w="100%">
        <TextInput
          label={<PrimaryText size="sm">{t("email_address")}</PrimaryText>}
          w="100%"
          {...emailField.getInputProps()}
          elevation={1}
        />
        <PasswordInput
          label={<PrimaryText size="sm">{t("password")}</PrimaryText>}
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
            emailField.validate();
            passwordField.validate();
            confirmPasswordField.validate();

            if (
              emailField.error ||
              passwordField.error ||
              confirmPasswordField.error
            ) {
              return;
            }

            registerMutation.mutate(
              {
                email: emailField.getValue(),
                password: passwordField.getValue(),
              },
              {
                onSuccess: () => {
                  props.setLoginCardState(LoginCardState.Login);
                },
              },
            );
          }}
        >
          {t("register")}
        </Button>
      </Stack>
      <Button
        variant="default"
        fullWidth
        onClick={() => props.setLoginCardState(LoginCardState.Login)}
      >
        {t("back_to_login")}
      </Button>
    </Stack>
  );
};

export default Register;
