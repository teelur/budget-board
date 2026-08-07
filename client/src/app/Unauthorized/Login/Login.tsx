import {
  Alert,
  Button,
  LoadingOverlay,
  Stack,
  Divider,
  Group,
} from "@mantine/core";
import { Info } from "lucide-react";
import { hasLength, isEmail, useField } from "@mantine/form";
import React from "react";
import { LoginCardState } from "../Welcome";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";
import { getProjectEnvVariables } from "~/shared/projectEnvVariables";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import PasswordInput from "~/components/core/Input/PasswordInput/PasswordInput";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import Checkbox from "~/components/core/Checkbox/Checkbox";
import { OidcAuthFlows } from "~/models/oidc";
import { useLoginMutation } from "~/hooks/mutations/auth/useLoginMutation";
import { useResendConfirmationEmailMutation } from "~/hooks/mutations/auth/useResendConfirmationEmailMutation";
import { useForgotPasswordMutation } from "~/hooks/mutations/auth/useForgotPasswordMutation";

interface LoginProps {
  setLoginCardState: React.Dispatch<React.SetStateAction<LoginCardState>>;
  setUserEmail: React.Dispatch<React.SetStateAction<string>>;
  setUserPassword: React.Dispatch<React.SetStateAction<string>>;
  rememberMe: boolean;
  setRememberMe: React.Dispatch<React.SetStateAction<boolean>>;
}

const Login = (props: LoginProps): React.ReactNode => {
  const { t } = useTranslation();
  const { setIsUserAuthenticated, startOidcLogin, oidcLoading } = useAuth();
  const queryClient = useQueryClient();
  const loginMutation = useLoginMutation();
  const resendConfirmationEmailMutation = useResendConfirmationEmailMutation();
  const forgotPasswordMutation = useForgotPasswordMutation();

  const { envVariables } = getProjectEnvVariables();

  const isDemoMode = envVariables.VITE_DEMO_MODE?.toLowerCase() === "true";

  const emailField = useField<string>({
    initialValue: isDemoMode ? "demo@example.com" : "",
    validate: isEmail(t("invalid_email_message")),
  });

  const passwordMinLength = 3;
  const passwordField = useField<string>({
    initialValue: isDemoMode ? "demo" : "",
    validate: hasLength(
      { min: passwordMinLength },
      t("password_min_length_message", {
        minLength: passwordMinLength,
      }),
    ),
  });

  const doLogin = (): void => {
    emailField.validate();
    passwordField.validate();

    if (emailField.error || passwordField.error) {
      return;
    }

    const email = emailField.getValue();
    const password = passwordField.getValue();

    loginMutation.mutate(
      {
        email,
        password,
        rememberMe: props.rememberMe,
      },
      {
        onSuccess: (response) => {
          if (response.data === "RequiresTwoFactor") {
            props.setLoginCardState(LoginCardState.LoginWith2fa);
            props.setUserEmail(email);
            props.setUserPassword(password);
            return;
          }

          setIsUserAuthenticated(true);
        },
        onError: (error) => {
          const axiosError = error as AxiosError;

          if (
            (axiosError.response?.data as any)?.detail ===
            "EmailNotVerifiedError"
          ) {
            notifications.show({
              color: "var(--button-color-destructive)",
              message: (
                <Group gap="1rem" wrap="nowrap">
                  <div>{t("login_account_not_verified_message")}</div>
                  <Button
                    size="xs"
                    miw="fit-content"
                    loading={resendConfirmationEmailMutation.isPending}
                    onClick={() =>
                      resendConfirmationEmailMutation.mutate(email)
                    }
                  >
                    {t("resend")}
                  </Button>
                </Group>
              ),
              autoClose: 10000,
            });
          } else if (
            (axiosError.response?.data as any)?.detail ===
            "InvalidEmailOrPasswordError"
          ) {
            notifications.show({
              color: "var(--button-color-destructive)",
              message: t("login_failed_message"),
            });
          } else {
            notifications.show({
              color: "var(--button-color-destructive)",
              message: translateAxiosError(axiosError),
            });
          }
        },
        onSettled: async () => {
          await queryClient.invalidateQueries();
        },
      },
    );
  };

  return (
    <Stack gap={0} align="center" w="100%">
      <LoadingOverlay
        visible={forgotPasswordMutation.isPending || loginMutation.isPending}
        zIndex={1000}
        overlayProps={{ radius: "sm", blur: 2 }}
      />
      {envVariables.VITE_DEMO_MODE?.toLowerCase() === "true" && (
        <Alert
          icon={<Info size={16} />}
          color="blue"
          title={t("demo_mode")}
          w="100%"
          p="1rem"
          radius={0}
        >
          {t("demo_mode_login_hint")}
        </Alert>
      )}
      {envVariables.VITE_DISABLE_LOCAL_AUTH?.toLowerCase() !== "true" && (
        <Stack w="100%" align="center" gap="0.75rem" pb={"0.5rem"} p={"1rem"}>
          <TextInput
            {...emailField.getInputProps()}
            label={<PrimaryText size="sm">{t("email_address")}</PrimaryText>}
            w="100%"
            elevation={1}
          />
          <PasswordInput
            {...passwordField.getInputProps()}
            label={<PrimaryText size="sm">{t("password")}</PrimaryText>}
            w="100%"
            elevation={1}
          />
          <Button variant="filled" fullWidth onClick={doLogin}>
            {t("login")}
          </Button>
          <Group justify="center" w="100%">
            <Button
              size="xs"
              variant="subtle"
              fw={600}
              onClick={() => {
                if (emailField.getValue()) {
                  forgotPasswordMutation.mutate(emailField.getValue(), {
                    onSuccess: () => {
                      props.setLoginCardState(LoginCardState.ResetPassword);
                      props.setUserEmail(emailField.getValue());
                    },
                  });
                } else {
                  notifications.show({
                    color: "var(--button-color-destructive)",
                    message: t("reset_password_missing_email_message"),
                  });
                }
              }}
            >
              {t("reset_password")}
            </Button>
          </Group>
        </Stack>
      )}
      {envVariables.VITE_OIDC_ENABLED?.toLowerCase() === "true" &&
        envVariables.VITE_DISABLE_LOCAL_AUTH?.toLowerCase() !== "true" && (
          <Divider w="100%" label={t("or")} />
        )}
      {envVariables.VITE_OIDC_ENABLED?.toLowerCase() === "true" && (
        <Stack w="100%" pt="0.5rem" p="1rem">
          <Button
            variant="outline"
            fullWidth
            onClick={() =>
              startOidcLogin &&
              startOidcLogin(props.rememberMe, OidcAuthFlows.SignIn)
            }
            loading={oidcLoading}
          >
            {t("login_with_oidc")}
          </Button>
        </Stack>
      )}
      <Divider w="100%" />
      <Stack w="100%" p="1rem">
        <Checkbox
          label={<PrimaryText size="sm">{t("remember_device")}</PrimaryText>}
          checked={props.rememberMe}
          onChange={(event) => props.setRememberMe(event.currentTarget.checked)}
          elevation={1}
        />
      </Stack>
    </Stack>
  );
};

export default Login;
