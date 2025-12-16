import { Stack, Button, LoadingOverlay } from "@mantine/core";
import { hasLength, isEmail, useField } from "@mantine/form";
import React from "react";
import { LoginCardState } from "../Welcome";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { notifications } from "@mantine/notifications";
import { translateAxiosError, ValidationError } from "~/helpers/requests";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import PasswordInput from "~/components/core/Input/PasswordInput/PasswordInput";
import { useTranslation } from "react-i18next";

interface RegisterProps {
  setLoginCardState: React.Dispatch<React.SetStateAction<LoginCardState>>;
}

const Register = (props: RegisterProps): React.ReactNode => {
  const [loading, setLoading] = React.useState(false);

  const { t } = useTranslation();

  const emailField = useField<string>({
    initialValue: "",
    validate: isEmail(t("unauthorized.login.error_invalid_email")),
  });

  const passwordMinLength = 3;
  const passwordField = useField<string>({
    initialValue: "",
    validate: hasLength(
      { min: passwordMinLength },
      t("unauthorized.common.error_password_min_length", {
        minLength: passwordMinLength,
      })
    ),
  });
  const confirmPasswordField = useField<string>({
    initialValue: "",
    validate: (value) =>
      value !== passwordField.getValue()
        ? t("unauthorized.common.error_passwords_do_not_match")
        : null,
  });

  const { request } = useAuth();

  const registerUser = async (): Promise<void> => {
    setLoading(true);

    emailField.validate();
    passwordField.validate();
    confirmPasswordField.validate();

    if (emailField.error || passwordField.error || confirmPasswordField.error) {
      setLoading(false);
      return;
    }

    try {
      await request({
        url: "/api/register",
        method: "POST",
        data: {
          email: emailField.getValue(),
          password: passwordField.getValue(),
        },
      });

      props.setLoginCardState(LoginCardState.Login);

      notifications.show({
        color: "var(--button-color-confirm)",
        message: t(
          "unauthorized.register.message_account_created_email_verification"
        ),
      });
    } catch (error: any) {
      if (
        error?.response?.data &&
        error.status === 400 &&
        (error.response.data as ValidationError).title ===
          "One or more validation errors occurred."
      ) {
        notifications.show({
          title: t("unauthorized.register.error_validation"),
          color: "var(--button-color-destructive)",
          message: Object.values(
            (error.response.data as ValidationError).errors
          ).join("\n"),
        });
      } else {
        notifications.show({
          color: "var(--button-color-destructive)",
          message: translateAxiosError(error),
        });
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <Stack gap="0.75rem" align="center">
      <LoadingOverlay
        visible={loading}
        zIndex={1000}
        overlayProps={{ blur: 2 }}
      />
      <Stack align="center" gap="0.5rem" w="100%">
        <TextInput
          label={
            <PrimaryText size="sm">
              {t("unauthorized.register.label_email_address")}
            </PrimaryText>
          }
          w="100%"
          {...emailField.getInputProps()}
          elevation={1}
        />
        <PasswordInput
          label={
            <PrimaryText size="sm">
              {t("unauthorized.register.label_password")}
            </PrimaryText>
          }
          w="100%"
          {...passwordField.getInputProps()}
          elevation={1}
        />
        <PasswordInput
          label={
            <PrimaryText size="sm">
              {t("unauthorized.register.label_confirm_password")}
            </PrimaryText>
          }
          w="100%"
          {...confirmPasswordField.getInputProps()}
          elevation={1}
        />
        <Button variant="filled" fullWidth onClick={registerUser}>
          {t("unauthorized.register.button_register")}
        </Button>
      </Stack>
      <Button
        variant="light"
        fullWidth
        onClick={() => props.setLoginCardState(LoginCardState.Login)}
      >
        {t("unauthorized.register.button_back_to_login")}
      </Button>
    </Stack>
  );
};

export default Register;
