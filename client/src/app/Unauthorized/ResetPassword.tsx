import { Stack, Button, LoadingOverlay } from "@mantine/core";
import { useForm, hasLength, isNotEmpty } from "@mantine/form";
import React from "react";
import { LoginCardState } from "./Welcome";

import classes from "./Welcome.module.css";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import PasswordInput from "~/components/core/Input/PasswordInput/PasswordInput";

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
          color: "var(--button-color-confirm)",
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
          label={<PrimaryText size="sm">Reset Code</PrimaryText>}
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
