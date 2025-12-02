import { hasLength, useField } from "@mantine/form";
import { Button, LoadingOverlay, Stack } from "@mantine/core";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { notifications } from "@mantine/notifications";
import { translateAxiosError, ValidationError } from "~/helpers/requests";
import { AxiosError } from "axios";
import Card from "~/components/Card/Card";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";
import PasswordInput from "~/components/Input/PasswordInput/PasswordInput";

const ResetPassword = (): React.ReactNode => {
  const oldPasswordField = useField<string>({
    initialValue: "",
    validate: hasLength({ min: 3 }, "Password must be at least 3 characters."),
  });
  const newPasswordField = useField<string>({
    initialValue: "",
    validate: hasLength({ min: 3 }, "Password must be at least 3 characters."),
  });
  const confirmNewPasswordField = useField<string>({
    initialValue: "",
    validate: (value: string) =>
      value !== newPasswordField.getValue() ? "Passwords did not match" : null,
  });

  type ResetPasswordData = {
    oldPassword: string;
    newPassword: string;
  };

  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doResetPassword = useMutation({
    mutationFn: async (resetPasswordData: ResetPasswordData) =>
      await request({
        url: "/api/manage/info",
        method: "POST",
        data: {
          newPassword: resetPasswordData.newPassword,
          oldPassword: resetPasswordData.oldPassword,
        },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["user"] });

      oldPasswordField.reset();
      newPasswordField.reset();
      confirmNewPasswordField.reset();

      notifications.show({
        color: "green",
        message: "Password successfully updated.",
      });
    },
    onError: (error: AxiosError) => {
      if (error?.response?.data) {
        const errorData = error.response.data as ValidationError;
        if (
          error.status === 400 &&
          errorData.title === "One or more validation errors occurred."
        ) {
          notifications.show({
            title: "One or more validation errors occurred.",
            color: "red",
            message: Object.values(errorData.errors).join("\n"),
          });
        }
      } else {
        notifications.show({
          color: "red",
          message: translateAxiosError(error),
        });
      }
    },
  });

  return (
    <Card elevation={1}>
      <LoadingOverlay visible={doResetPassword.isPending} />
      <Stack gap="1rem">
        <PrimaryText size="lg">Reset Password</PrimaryText>
        <PasswordInput
          {...oldPasswordField.getInputProps()}
          label={<PrimaryText size="sm">Current Password</PrimaryText>}
          w="100%"
          elevation={1}
        />
        <PasswordInput
          {...newPasswordField.getInputProps()}
          label={<PrimaryText size="sm">New Password</PrimaryText>}
          w="100%"
          elevation={1}
        />
        <PasswordInput
          {...confirmNewPasswordField.getInputProps()}
          label={<PrimaryText size="sm">Confirm New Password</PrimaryText>}
          w="100%"
          elevation={1}
        />
        <Button
          onClick={() => {
            oldPasswordField.validate();
            newPasswordField.validate();
            confirmNewPasswordField.validate();

            if (
              !oldPasswordField.error &&
              !newPasswordField.error &&
              !confirmNewPasswordField.error
            ) {
              doResetPassword.mutate({
                oldPassword: oldPasswordField.getValue(),
                newPassword: newPasswordField.getValue(),
              } as ResetPasswordData);
            }
          }}
        >
          Submit
        </Button>
      </Stack>
    </Card>
  );
};

export default ResetPassword;
