import { Button, LoadingOverlay, Stack, Group } from "@mantine/core";
import { useField } from "@mantine/form";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PinInput from "~/components/core/Input/PinInput/PinInput";
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
      notifications.show({
        color: "var(--button-color-destructive)",
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
    <Stack gap="md" align="center" w="100%" p="1rem">
      <LoadingOverlay
        visible={loginMutation.isPending}
        zIndex={1000}
        overlayProps={{ radius: "sm", blur: 2 }}
      />
      <Stack align="center" gap={5} w="100%">
        <PrimaryText size="lg" ta="center">
          {t("two_factor_authentication")}
        </PrimaryText>
        <DimmedText size="sm" ta="center">
          {t("enter_security_code_message")}
        </DimmedText>
      </Stack>
      <PinInput
        length={6}
        type="number"
        oneTimeCode
        autoFocus
        value={authenticationCodeField.getValue()}
        onChange={(value) => authenticationCodeField.setValue(value)}
        elevation={1}
      />
      <Button variant="filled" fullWidth onClick={submitUserLogin}>
        {t("submit")}
      </Button>
      <Group wrap="nowrap" gap="md" w="100%">
        <Button
          variant="default"
          fullWidth
          onClick={() =>
            props.setLoginCardState(LoginCardState.LoginWithRecovery)
          }
        >
          {t("use_recovery_code")}
        </Button>
        <Button
          variant="default"
          fullWidth
          onClick={() => props.setLoginCardState(LoginCardState.Login)}
        >
          {t("return_to_login")}
        </Button>
      </Group>
    </Stack>
  );
};

export default LoginWith2fa;
