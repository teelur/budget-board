import { Group, Stack } from "@mantine/core";
import {
  BodyText,
  Button,
  Card,
  HeadingText,
  TextInput,
} from "@teelur/budget-board-ui";
import { useField } from "@mantine/form";
import React from "react";
import { LoginCardState } from "../Welcome";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { NotificationType, showNotification } from "~/helpers/notifications";
import { useTranslation } from "react-i18next";
import { useLoginMutation } from "~/hooks/mutations/auth/useLoginMutation";

interface LoginProps {
  setLoginCardState: React.Dispatch<React.SetStateAction<LoginCardState>>;
  userEmail: string;
  userPassword: string;
  rememberMe: boolean;
}

const LoginWithRecovery = (props: LoginProps): React.ReactNode => {
  const { t } = useTranslation();

  const recoveryCodeField = useField<string>({
    initialValue: "",
  });

  const { setIsUserAuthenticated } = useAuth();
  const queryClient = useQueryClient();
  const loginMutation = useLoginMutation();

  const submitUserLogin = (): void => {
    if (!recoveryCodeField.getValue()) {
      showNotification({
        type: NotificationType.Error,
        message: t("enter_recovery_code_message"),
      });
      return;
    }

    const recoveryCode = recoveryCodeField.getValue();

    loginMutation.mutate(
      {
        email: props.userEmail,
        password: props.userPassword,
        rememberMe: props.rememberMe,
        recoveryCode,
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
      <Stack gap="1rem" align="center">
        <Stack align="center" gap={5} w="100%">
          <HeadingText level={5} ta="center">
            {t("use_a_recovery_code")}
          </HeadingText>
          <BodyText component="p" size="sm" ta="center">
            {t("enter_recovery_code_subheading")}
          </BodyText>
        </Stack>
        <TextInput {...recoveryCodeField.getInputProps()} w="100%" />
        <Group gap="0.5rem" w="100%">
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
            onClick={submitUserLogin}
            loading={loginMutation.isPending}
          >
            {t("submit")}
          </Button>
        </Group>
      </Stack>
    </Card.Section>
  );
};

export default LoginWithRecovery;
