import {
  Button,
  LoadingOverlay,
  Stack,
  Text,
  PinInput,
  Group,
} from "@mantine/core";
import { useField } from "@mantine/form";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";
import { LoginCardState } from "./Welcome";

interface LoginProps {
  setLoginCardState: React.Dispatch<React.SetStateAction<LoginCardState>>;
  userEmail: string;
  userPassword: string;
}

const LoginWith2fa = (props: LoginProps): React.ReactNode => {
  const [loading, setLoading] = React.useState(false);

  const authenticationCodeField = useField<string>({
    initialValue: "",
    validate: (value) => {
      if (!value) {
        return "Authentication code is required";
      }
      return null;
    },
  });

  const { request, setAccessToken } = React.useContext<any>(AuthContext);

  const queryClient = useQueryClient();

  const submitUserLogin = async (): Promise<void> => {
    setLoading(true);

    if (!authenticationCodeField.getValue()) {
      notifications.show({
        color: "red",
        message: "Please enter the authentication code.",
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
      .then((res: AxiosResponse) => {
        setAccessToken(res.data.accessToken);
        localStorage.setItem("refresh-token", res.data.refreshToken);
      })
      .catch((error: AxiosError) => {
        // These error response values are specific to ASP.NET Identity,
        // so will do the error translation here.
        if ((error.response?.data as any)?.detail === "NotAllowed") {
          notifications.show({
            color: "red",
            message:
              "Please check your email for a validation email before logging in.",
          });
        } else if ((error.response?.data as any)?.detail === "Failed") {
          notifications.show({
            color: "red",
            message: "Login failed. Check your credentials and try again.",
          });
        } else {
          notifications.show({
            color: "red",
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
      <Text>Enter the 6-digit security code from your authenticator app.</Text>
      <PinInput
        length={6}
        type="number"
        autoFocus
        value={authenticationCodeField.getValue()}
        onChange={(value) => authenticationCodeField.setValue(value)}
      />
      <Button variant="filled" fullWidth onClick={submitUserLogin}>
        Submit
      </Button>
      <Group wrap="nowrap" gap="md" w="100%">
        <Button
          variant="default"
          fullWidth
          onClick={() => props.setLoginCardState(LoginCardState.Login)}
        >
          Use Recovery Code
        </Button>
        <Button
          variant="default"
          fullWidth
          onClick={() => props.setLoginCardState(LoginCardState.Login)}
        >
          Return to Login
        </Button>
      </Group>
    </Stack>
  );
};

export default LoginWith2fa;
