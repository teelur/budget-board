import { Stack, Button, LoadingOverlay } from "@mantine/core";
import { hasLength, useField } from "@mantine/form";
import React from "react";
import { LoginCardState } from "../Welcome";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import PasswordInput from "~/components/core/Input/PasswordInput/PasswordInput";
import { useTranslation } from "react-i18next";

interface ResetPasswordProps {
  setLoginCardState: React.Dispatch<React.SetStateAction<LoginCardState>>;
  email: string;
}

const ResetPassword = (props: ResetPasswordProps): React.ReactNode => {
  const [loading, setLoading] = React.useState(false);

  const { t } = useTranslation();

  const resetCodeField = useField<string>({
    initialValue: "",
    validate: (value) =>
      value.trim() === ""
        ? t("unauthorized.reset_password.error_reset_code_required")
        : null,
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

  const updatePassword = async (): Promise<void> => {
    setLoading(true);

    resetCodeField.validate();
    passwordField.validate();
    confirmPasswordField.validate();

    if (
      resetCodeField.error ||
      passwordField.error ||
      confirmPasswordField.error
    ) {
      setLoading(false);
      return;
    }

    try {
      await request({
        url: "/api/resetPassword",
        method: "POST",
        data: {
          email: props.email,
          resetCode: resetCodeField.getValue(),
          newPassword: passwordField.getValue(),
        },
      });

      props.setLoginCardState(LoginCardState.Login);

      notifications.show({
        color: "var(--button-color-confirm)",
        message: t(
          "unauthorized.reset_password.message_password_reset_success"
        ),
      });
    } catch (error: any) {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <Stack gap="0.75rem" align="center">
      <LoadingOverlay
        visible={loading}
        zIndex={1000}
        overlayProps={{ radius: "sm", blur: 2 }}
      />
      <Stack align="center" gap="0.5rem" w="100%">
        <TextInput
          label={
            <PrimaryText size="sm">
              {t("unauthorized.reset_password.label_reset_code")}
            </PrimaryText>
          }
          w="100%"
          {...resetCodeField.getInputProps()}
          elevation={1}
        />
        <PasswordInput
          label={
            <PrimaryText size="sm">
              {t("unauthorized.reset_password.label_new_password")}
            </PrimaryText>
          }
          w="100%"
          {...passwordField.getInputProps()}
          elevation={1}
        />
        <PasswordInput
          label={
            <PrimaryText size="sm">
              {t("unauthorized.reset_password.label_confirm_password")}
            </PrimaryText>
          }
          w="100%"
          {...confirmPasswordField.getInputProps()}
          elevation={1}
        />
        <Button variant="filled" fullWidth onClick={updatePassword}>
          {t("unauthorized.reset_password.button_reset_password")}
        </Button>
      </Stack>
      <Button
        variant="default"
        fullWidth
        onClick={() => props.setLoginCardState(LoginCardState.Login)}
      >
        {t("unauthorized.reset_password.button_return_to_login")}
      </Button>
    </Stack>
  );
};

export default ResetPassword;
