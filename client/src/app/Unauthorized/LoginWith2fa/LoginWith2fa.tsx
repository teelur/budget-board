import { Stack, Group } from "@mantine/core";
import {
  BodyText,
  Button,
  Card,
  HeadingText,
  PinInput,
} from "@teelur/budget-board-ui";
import { useField } from "@mantine/form";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { NotificationType, showNotification } from "~/helpers/notifications";
import { useTranslation } from "react-i18next";
import { LoginCardState } from "../Welcome";
import { useLoginMutation } from "~/hooks/mutations/auth/useLoginMutation";

interface LoginProps {
  setLoginCardState: React.Dispatch<React.SetStateAction<LoginCardState>>;
  userEmail: string;
  userPassword: string;
  rememberMe: boolean;
}

const LoginWith2fa = (props: LoginProps): React.ReactNode => {
  const { t } = useTranslation();

  const authenticationCodeField = useField<string>({
    initialValue: "",
  });

  const { setIsUserAuthenticated } = useAuth();
  const queryClient = useQueryClient();
  const loginMutation = useLoginMutation();

  const submitUserLogin = (): void => {
    if (!authenticationCodeField.getValue()) {
      showNotification({
        type: NotificationType.Error,
        message: t("enter_authentication_code_message"),
      });
      return;
    }

    const authenticationCode = authenticationCodeField.getValue();

    loginMutation.mutate(
      {
        email: props.userEmail,
        password: props.userPassword,
        rememberMe: props.rememberMe,
        twoFactorCode: authenticationCode,
      },
      {
        onSuccess: () => {
          setIsUserAuthenticated(true);
        },
        onError: (error) => {
          const axiosError = error as AxiosError;

          if ((axiosError.response?.data as any)?.detail === "Failed") {
            showNotification({
              type: NotificationType.Error,
              message: t("login_failed_message"),
            });
          } else {
            showNotification({
              type: NotificationType.Error,
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
    <Card.Section>
      <Stack gap="1rem" align="center" w="100%">
        <Stack align="center" gap={5} w="100%">
          <HeadingText level={5} ta="center">
            {t("two_factor_authentication")}
          </HeadingText>
          <BodyText component="p" tone="secondary" size="sm">
            {t("enter_security_code_message")}
          </BodyText>
        </Stack>
        <PinInput
          length={6}
          type="number"
          inputMode="numeric"
          oneTimeCode
          ariaLabel={t("enter_security_code_message")}
          getInputProps={(index) =>
            index === 0 ? { name: "two-factor-code" } : {}
          }
          autoFocus
          value={authenticationCodeField.getValue()}
          onChange={(value) => authenticationCodeField.setValue(value)}
          onKeyDown={(event) => {
            if (event.key === "Enter") {
              event.preventDefault();
              submitUserLogin();
            }
          }}
        />
        <Button
          variant="filled"
          color="primary"
          size="compact-md"
          fullWidth
          loading={loginMutation.isPending}
          onClick={submitUserLogin}
        >
          {t("submit")}
        </Button>
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
            color="secondary"
            size="compact-sm"
            flex="1 1 0"
            onClick={() =>
              props.setLoginCardState(LoginCardState.LoginWithRecovery)
            }
          >
            {t("use_recovery_code")}
          </Button>
        </Group>
      </Stack>
    </Card.Section>
  );
};

export default LoginWith2fa;
