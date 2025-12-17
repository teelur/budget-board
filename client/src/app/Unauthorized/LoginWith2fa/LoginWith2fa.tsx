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

interface LoginProps {
  setLoginCardState: React.Dispatch<React.SetStateAction<LoginCardState>>;
  userEmail: string;
  userPassword: string;
}

const LoginWith2fa = (props: LoginProps): React.ReactNode => {
  const [loading, setLoading] = React.useState(false);

  const { t } = useTranslation();

  const authenticationCodeField = useField<string>({
    initialValue: "",
  });

  const { request, setIsUserAuthenticated } = useAuth();

  const queryClient = useQueryClient();

  const submitUserLogin = async (): Promise<void> => {
    setLoading(true);

    if (!authenticationCodeField.getValue()) {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: t("enter_authentication_code_message"),
      });
      setLoading(false);
      return;
    }

    request({
      url: "/api/login",
      method: "POST",
      data: {
        email: props.userEmail,
        password: props.userPassword,
        twoFactorCode: authenticationCodeField.getValue(),
      },
    })
      .then(() => {
        setIsUserAuthenticated(true);
      })
      .catch((error: AxiosError) => {
        // These error response values are specific to ASP.NET Identity,
        // so will do the error translation here.
        if ((error.response?.data as any)?.detail === "Failed") {
          notifications.show({
            color: "var(--button-color-destructive)",
            message: t("login_failed_message"),
          });
        } else {
          notifications.show({
            color: "var(--button-color-destructive)",
            message: translateAxiosError(error),
          });
        }
      })
      .finally(() => {
        // Invalidate all old queries, so we refetch for new user.
        queryClient.invalidateQueries();
        setLoading(false);
      });
  };

  return (
    <Stack gap="md" align="center" w="100%">
      <LoadingOverlay
        visible={loading}
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
