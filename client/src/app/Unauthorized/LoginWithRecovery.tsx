import {
  Button,
  LoadingOverlay,
  Stack,
  TextInput,
  Text,
  Title,
} from "@mantine/core";
import { useField } from "@mantine/form";
import React from "react";
import { LoginCardState } from "./Welcome";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";

interface LoginProps {
  setLoginCardState: React.Dispatch<React.SetStateAction<LoginCardState>>;
  userEmail: string;
  userPassword: string;
}

const LoginWithRecovery = (props: LoginProps): React.ReactNode => {
  const [loading, setLoading] = React.useState(false);

  const recoveryCodeField = useField<string>({
    initialValue: "",
    validate: (value) => {
      if (!value) {
        return "Recovery code is required";
      }
      return null;
    },
  });

  const { request, setIsUserAuthenticated } =
    React.useContext<any>(AuthContext);

  const queryClient = useQueryClient();

  const submitUserLogin = async (): Promise<void> => {
    setLoading(true);

    if (!recoveryCodeField.getValue()) {
      notifications.show({
        color: "red",
        message: "Please enter the recovery code.",
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
    <Stack gap="md" align="center">
      <LoadingOverlay
        visible={loading}
        zIndex={1000}
        overlayProps={{ radius: "sm", blur: 2 }}
      />
      <Stack align="center" gap={5} w="100%">
        <Title order={3} ta="center">
          Use a Recovery Code
        </Title>
        <Text size="sm" ta="center">
          Enter one of your recovery codes for a one-time password
          authentication.
        </Text>
      </Stack>
      <TextInput {...recoveryCodeField.getInputProps()} w="100%" />
      <Stack gap="0.5rem" w="100%">
        <Button variant="filled" fullWidth onClick={submitUserLogin}>
          Submit
        </Button>
        <Button
          variant="default"
          fullWidth
          onClick={() => props.setLoginCardState(LoginCardState.Login)}
        >
          Return to Login
        </Button>
      </Stack>
    </Stack>
  );
};

export default LoginWithRecovery;
