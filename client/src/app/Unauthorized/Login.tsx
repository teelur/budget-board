import {
  Anchor,
  Button,
  LoadingOverlay,
  PasswordInput,
  Stack,
  TextInput,
} from "@mantine/core";
import { hasLength, isEmail, useForm } from "@mantine/form";
import React from "react";
import classes from "./Welcome.module.css";
import { LoginCardState } from "./Welcome";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";

interface LoginProps {
  setLoginCardState: React.Dispatch<React.SetStateAction<LoginCardState>>;
  setUserEmail: React.Dispatch<React.SetStateAction<string>>;
}

const Login = (props: LoginProps): React.ReactNode => {
  const [loading, setLoading] = React.useState(false);

  const form = useForm({
    mode: "controlled",
    initialValues: { email: "", password: "" },
    validate: {
      email: isEmail("Invalid email"),
      password: hasLength({ min: 3 }, "Must be at least 3 characters"),
    },
  });

  const { request, setAccessToken } = React.useContext<any>(AuthContext);

  const queryClient = useQueryClient();

  const submitUserLogin = async (
    values: typeof form.values,
    e: any
  ): Promise<void> => {
    e.preventDefault();
    setLoading(true);

    request({
      url: "/api/login",
      method: "POST",
      data: {
        email: values.email,
        password: values.password,
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
      <form
        className={classes.form}
        style={{ width: "100%" }}
        onSubmit={form.onSubmit(submitUserLogin)}
      >
        <TextInput
          {...form.getInputProps("email")}
          key={form.key("email")}
          label="Email"
          w="100%"
        />
        <PasswordInput
          {...form.getInputProps("password")}
          key={form.key("password")}
          label="Password"
          w="100%"
        />
        <Button variant="filled" fullWidth type="submit">
          Login
        </Button>
      </form>
      <Anchor
        size="sm"
        onClick={submitPasswordReset.bind(null, form.values.email)}
      >
        Reset Password
      </Anchor>
    </Stack>
  );
};

export default Login;
