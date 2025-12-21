import { Button, LoadingOverlay, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import React from "react";
import { LoginCardState } from "../Welcome";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import { useTranslation } from "react-i18next";

interface LoginProps {
  setLoginCardState: React.Dispatch<React.SetStateAction<LoginCardState>>;
  userEmail: string;
  userPassword: string;
}

const LoginWithRecovery = (props: LoginProps): React.ReactNode => {
  const [loading, setLoading] = React.useState(false);

  const { t } = useTranslation();

  const recoveryCodeField = useField<string>({
    initialValue: "",
  });

  const { request, setIsUserAuthenticated } = useAuth();

  const queryClient = useQueryClient();

  const submitUserLogin = async (): Promise<void> => {
    setLoading(true);

    if (!recoveryCodeField.getValue()) {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: t("enter_recovery_code_message"),
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
        recoveryCode: recoveryCodeField.getValue(),
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
    <Stack gap="md" align="center">
      <LoadingOverlay
        visible={loading}
        zIndex={1000}
        overlayProps={{ radius: "sm", blur: 2 }}
      />
      <Stack align="center" gap={5} w="100%">
        <PrimaryText size="md" ta="center">
          {t("use_a_recovery_code")}
        </PrimaryText>
        <DimmedText size="sm" ta="center">
          {t("enter_recovery_code_subheading")}
        </DimmedText>
      </Stack>
      <TextInput
        {...recoveryCodeField.getInputProps()}
        w="100%"
        elevation={1}
      />
      <Stack gap="0.5rem" w="100%">
        <Button variant="filled" fullWidth onClick={submitUserLogin}>
          {t("submit")}
        </Button>
        <Button
          variant="default"
          fullWidth
          onClick={() => props.setLoginCardState(LoginCardState.Login)}
        >
          {t("return_to_login")}
        </Button>
      </Stack>
    </Stack>
  );
};

export default LoginWithRecovery;
