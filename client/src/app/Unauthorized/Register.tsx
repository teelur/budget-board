import { Stack, Button, LoadingOverlay } from "@mantine/core";
import { hasLength, isEmail, useForm } from "@mantine/form";
import React from "react";
import { LoginCardState } from "./Welcome";

import classes from "./Welcome.module.css";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { notifications } from "@mantine/notifications";
import { AxiosError } from "axios";
import { translateAxiosError, ValidationError } from "~/helpers/requests";
import TextInput from "~/components/Input/TextInput/TextInput";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";
import PasswordInput from "~/components/Input/PasswordInput/PasswordInput";

interface RegisterProps {
  setLoginCardState: React.Dispatch<React.SetStateAction<LoginCardState>>;
}

const Register = (props: RegisterProps): React.ReactNode => {
  const [loading, setLoading] = React.useState(false);

  const form = useForm({
    mode: "uncontrolled",
    initialValues: { email: "", password: "", confirmPassword: "" },
    validate: {
      email: isEmail("Invalid email"),
      password: hasLength({ min: 3 }, "Must be at least 3 characters"),
      confirmPassword: (value, values) =>
        value !== values.password ? "Passwords did not match" : null,
    },
  });

  const { request } = useAuth();

  const submitUserRegister = async (
    values: typeof form.values,
    e: any
  ): Promise<void> => {
    e.preventDefault();
    setLoading(true);

    request({
      url: "/api/register",
      method: "POST",
      data: {
        email: values.email,
        password: values.password,
      },
    })
      .then(() => {
        props.setLoginCardState(LoginCardState.Login);

        notifications.show({
          color: "green",
          message:
            "Account created. Check your email for a verification message.",
        });
      })
      .catch((error: AxiosError) => {
        if (
          error?.response?.data &&
          error.status === 400 &&
          (error.response.data as ValidationError).title ===
            "One or more validation errors occurred."
        ) {
          notifications.show({
            title: "One or more validation errors occurred.",
            color: "red",
            message: Object.values(
              (error.response.data as ValidationError).errors
            ).join("\n"),
          });
        } else {
          notifications.show({
            color: "red",
            message: translateAxiosError(error),
          });
        }
      })
      .finally(() => {
        setLoading(false);
      });
  };

  return (
    <Stack gap="md" align="center">
      <LoadingOverlay
        visible={loading}
        zIndex={1000}
        overlayProps={{ blur: 2 }}
      />
      <form
        className={classes.form}
        style={{ width: "100%" }}
        onSubmit={form.onSubmit(submitUserRegister)}
      >
        <TextInput
          {...form.getInputProps("email")}
          key={form.key("email")}
          label={<PrimaryText size="sm">Email Address</PrimaryText>}
          w="100%"
          elevation={1}
        />
        <PasswordInput
          {...form.getInputProps("password")}
          key={form.key("password")}
          label={<PrimaryText size="sm">Password</PrimaryText>}
          w="100%"
          elevation={1}
        />
        <PasswordInput
          {...form.getInputProps("confirmPassword")}
          key={form.key("confirmPassword")}
          label={<PrimaryText size="sm">Confirm Password</PrimaryText>}
          w="100%"
          elevation={1}
        />
        <Button variant="filled" fullWidth type="submit">
          Register
        </Button>
      </form>
      <Button
        variant="light"
        fullWidth
        onClick={() => props.setLoginCardState(LoginCardState.Login)}
      >
        Return to Login
      </Button>
    </Stack>
  );
};

export default Register;
