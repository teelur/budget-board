import {
  Anchor,
  Button,
  LoadingOverlay,
  PasswordInput,
  Stack,
  TextInput,
  Divider,
} from "@mantine/core";
import { hasLength, isEmail, useField } from "@mantine/form";
import React from "react";
import { LoginCardState } from "./Welcome";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";
import { getProjectEnvVariables } from "~/shared/projectEnvVariables";

interface LoginProps {
  setLoginCardState: React.Dispatch<React.SetStateAction<LoginCardState>>;
  setUserEmail: React.Dispatch<React.SetStateAction<string>>;
  setUserPassword: React.Dispatch<React.SetStateAction<string>>;
}

const Login = (props: LoginProps): React.ReactNode => {
  const [loading, setLoading] = React.useState(false);

  const { envVariables } = getProjectEnvVariables();

  const emailField = useField<string>({
    initialValue: "",
    validate: isEmail("Invalid email"),
  });

  const passwordField = useField<string>({
    initialValue: "",
    validate: hasLength({ min: 3 }, "Must be at least 3 characters"),
  });

  const { request, setIsUserAuthenticated, startOidcLogin } = useAuth();

  const queryClient = useQueryClient();

  const doLogin = async (): Promise<void> => {
    setLoading(true);

    const isEmailValid = emailField.validate();
    const isPasswordValid = passwordField.validate();

    if (!isEmailValid || !isPasswordValid) {
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
            color: "green",
            message: "An email has been set with a reset code.",
          });
        })
        .catch(() => {
          notifications.show({
            color: "red",
            message: "There was an error resetting your password.",
          });
        })
        .finally(() => {
          setLoading(false);
        });
    } else {
      notifications.show({
        color: "red",
        message: "Please enter your email to reset your password.",
      });
    }
  };

  return (
    <Stack gap="md" align="center">
      <LoadingOverlay
        visible={loading}
        zIndex={1000}
        overlayProps={{ radius: "sm", blur: 2 }}
      />
      {envVariables.VITE_DISABLE_LOCAL_AUTH?.toLowerCase() !== "true" && (
        <Stack w="100%" align="center" gap="1rem">
          <TextInput {...emailField.getInputProps()} label="Email" w="100%" />
          <PasswordInput
            {...passwordField.getInputProps()}
            label="Password"
            w="100%"
          />
          <Button variant="filled" fullWidth onClick={doLogin}>
            Login
          </Button>
          <Anchor
            size="sm"
            fw={600}
            onClick={submitPasswordReset.bind(null, emailField.getValue())}
          >
            Reset Password
          </Anchor>
        </Stack>
      )}
      {envVariables.VITE_OIDC_ENABLED?.toLowerCase() === "true" &&
        envVariables.VITE_DISABLE_LOCAL_AUTH?.toLowerCase() !== "true" && (
          <Divider w="100%" label="or" />
        )}
      {envVariables.VITE_OIDC_ENABLED?.toLowerCase() === "true" && (
        <Button
          variant="outline"
          fullWidth
          onClick={() => startOidcLogin && startOidcLogin()}
        >
          Sign in with OIDC
        </Button>
      )}
    </Stack>
  );
};

export default Login;
