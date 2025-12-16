import { Anchor, Button, LoadingOverlay, Stack, Divider } from "@mantine/core";
import { hasLength, isEmail, useField } from "@mantine/form";
import React from "react";
import { LoginCardState } from "./Welcome";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";
import { getProjectEnvVariables } from "~/shared/projectEnvVariables";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import PasswordInput from "~/components/core/Input/PasswordInput/PasswordInput";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";

interface LoginProps {
  setLoginCardState: React.Dispatch<React.SetStateAction<LoginCardState>>;
  setUserEmail: React.Dispatch<React.SetStateAction<string>>;
  setUserPassword: React.Dispatch<React.SetStateAction<string>>;
}

const Login = (props: LoginProps): React.ReactNode => {
  const [loading, setLoading] = React.useState(false);

  const { t } = useTranslation();

  const { envVariables } = getProjectEnvVariables();

  const emailField = useField<string>({
    initialValue: "",
    validate: isEmail(t("unauthorized.login.error_invalid_email")),
  });

  const passwordMinLength = 3;
  const passwordField = useField<string>({
    initialValue: "",
    validate: hasLength(
      { min: passwordMinLength },
      t("unauthorized.login.error_password_min_length", {
        minLength: passwordMinLength,
      })
    ),
  });

  const { request, setIsUserAuthenticated, startOidcLogin, oidcLoading } =
    useAuth();

  const queryClient = useQueryClient();

  const doLogin = async (): Promise<void> => {
    setLoading(true);

    emailField.validate();
    passwordField.validate();

    if (emailField.error || passwordField.error) {
      setLoading(false);
      return;
    }

    request({
      url: "/api/login",
      method: "POST",
      data: {
        email: emailField.getValue(),
        password: passwordField.getValue(),
      },
    })
      .then((res: AxiosResponse) => {
        if (res.data === "RequiresTwoFactor") {
          props.setLoginCardState(LoginCardState.LoginWith2fa);
          props.setUserEmail(emailField.getValue());
          props.setUserPassword(passwordField.getValue());
          return;
        }

        setIsUserAuthenticated(true);
      })
      .catch((error: AxiosError) => {
        // These error response values are specific to ASP.NET Identity,
        // so will do the error translation here.
        if ((error.response?.data as any)?.detail === "NotAllowed") {
          notifications.show({
            color: "var(--button-color-destructive)",
            message: t("unauthorized.login.error_account_not_verified"),
          });
        } else if ((error.response?.data as any)?.detail === "Failed") {
          notifications.show({
            color: "var(--button-color-destructive)",
            message: t("unauthorized.login.error_login_failed"),
          });
        } else {
          notifications.show({
            color: "var(--button-color-destructive)",
            message: translateAxiosError(error),
          });
        }
      })
      .finally(() => {
        queryClient.invalidateQueries();
        setLoading(false);
      });
  };

  const submitPasswordReset = (email: string): void => {
    if (email) {
      setLoading(true);
      request({
        url: "/api/forgotPassword",
        method: "POST",
        data: {
          email,
        },
      })
        .then(() => {
          props.setLoginCardState(LoginCardState.ResetPassword);
          props.setUserEmail(email);

          notifications.show({
            color: "var(--button-color-confirm)",
            message: t("unauthorized.login.message_reset_password_success"),
          });
        })
        .catch(() => {
          notifications.show({
            color: "var(--button-color-destructive)",
            message: t("unauthorized.login.error_reset_password"),
          });
        })
        .finally(() => {
          setLoading(false);
        });
    } else {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: t("unauthorized.login.error_reset_password_no_email"),
      });
    }
  };

  return (
    <Stack gap="1rem" align="center">
      <LoadingOverlay
        visible={loading}
        zIndex={1000}
        overlayProps={{ radius: "sm", blur: 2 }}
      />
      {envVariables.VITE_DISABLE_LOCAL_AUTH?.toLowerCase() !== "true" && (
        <Stack w="100%" align="center" gap="0.75rem">
          <TextInput
            {...emailField.getInputProps()}
            label={
              <PrimaryText size="sm">
                {t("unauthorized.login.label_email_address")}
              </PrimaryText>
            }
            w="100%"
            elevation={1}
          />
          <PasswordInput
            {...passwordField.getInputProps()}
            label={
              <PrimaryText size="sm">
                {t("unauthorized.login.label_password")}
              </PrimaryText>
            }
            w="100%"
            elevation={1}
          />
          <Button variant="filled" fullWidth onClick={doLogin}>
            {t("unauthorized.login.button_login")}
          </Button>
          <Anchor
            size="sm"
            fw={600}
            onClick={submitPasswordReset.bind(null, emailField.getValue())}
          >
            {t("unauthorized.login.button_reset_password")}
          </Anchor>
        </Stack>
      )}
      {envVariables.VITE_OIDC_ENABLED?.toLowerCase() === "true" &&
        envVariables.VITE_DISABLE_LOCAL_AUTH?.toLowerCase() !== "true" && (
          <Divider w="100%" label={t("unauthorized.login.divider_or")} />
        )}
      {envVariables.VITE_OIDC_ENABLED?.toLowerCase() === "true" && (
        <Button
          variant="outline"
          fullWidth
          onClick={() => startOidcLogin && startOidcLogin()}
          loading={oidcLoading}
        >
          {t("unauthorized.login.button_log_in_with_oidc")}
        </Button>
      )}
    </Stack>
  );
};

export default Login;
