import {
  Stack,
  TextInput,
  PasswordInput,
  Button,
  LoadingOverlay,
} from "@mantine/core";
import { useForm, hasLength, isNotEmpty } from "@mantine/form";
import React from "react";
import { LoginCardState } from "./Welcome";

import classes from "./Welcome.module.css";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";

interface ResetPasswordProps {
  setLoginCardState: React.Dispatch<React.SetStateAction<LoginCardState>>;
  email: string;
}

const ResetPassword = (props: ResetPasswordProps): React.ReactNode => {
  const [loading, setLoading] = React.useState(false);

  const form = useForm({
    mode: "uncontrolled",
    initialValues: { resetCode: "", password: "", confirmPassword: "" },
    validate: {
      resetCode: isNotEmpty("Reset code is required"),
      password: hasLength({ min: 3 }, "Must be at least 3 characters"),
      confirmPassword: (value, values) =>
        value !== values.password ? "Passwords did not match" : null,
    },
  });

  const { request } = useAuth();

  const submitPasswordUpdate = async (
    values: typeof form.values,
    e: any
  ): Promise<void> => {
    e.preventDefault();
    setLoading(true);

    request({
      url: "/api/resetPassword",
      method: "POST",
      data: {
        email: props.email,
        resetCode: values.resetCode,
        newPassword: values.password,
      },
    })
      .then(() => {
        props.setLoginCardState(LoginCardState.Login);

        notifications.show({
          color: "green",
          message: "Password successfully updated. Please log in.",
        });
      })
      .catch((error: AxiosError) => {
        notifications.show({
          color: "red",
          message: translateAxiosError(error),
        });
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
        overlayProps={{ radius: "sm", blur: 2 }}
      />
      <form
        className={classes.form}
        style={{ width: "100%" }}
        onSubmit={form.onSubmit(submitPasswordUpdate)}
      >
        <TextInput
          {...form.getInputProps("resetCode")}
          key={form.key("resetCode")}
          label="Reset Code"
          w="100%"
        />
        <PasswordInput
          {...form.getInputProps("password")}
          key={form.key("password")}
          label="Password"
          w="100%"
        />
        <PasswordInput
          {...form.getInputProps("confirmPassword")}
          key={form.key("confirmPassword")}
          label="Confirm Password"
          w="100%"
        />
        <Button variant="filled" fullWidth type="submit">
          Reset Password
        </Button>
      </form>
      <Button
        variant="default"
        fullWidth
        onClick={() => props.setLoginCardState(LoginCardState.Login)}
      >
        Return to Login
      </Button>
    </Stack>
  );
};

export default ResetPassword;
