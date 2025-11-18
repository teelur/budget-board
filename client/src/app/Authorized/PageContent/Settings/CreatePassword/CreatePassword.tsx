import { Button, Card, PasswordInput, Stack, Text } from "@mantine/core";
import { hasLength, useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError, ValidationError } from "~/helpers/requests";

const CreatePassword = (): React.ReactNode => {
  const newPasswordField = useField<string>({
    initialValue: "",
    validate: hasLength({ min: 3 }, "Password must be at least 3 characters."),
  });
  const confirmNewPasswordField = useField<string>({
    initialValue: "",
    validate: (value: string) =>
      value !== newPasswordField.getValue() ? "Passwords did not match" : null,
  });

  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doCreatePassword = useMutation({
    mutationFn: async () =>
      await request({
        url: "/api/manage/info",
        method: "POST",
        data: {
          newPassword: newPasswordField.getValue(),
        },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["user"] });
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
    <Card p="0.5rem" radius="md" shadow="sm" withBorder>
      <Stack gap="1rem">
        <Text fw={700} size="lg">
          Create Password
        </Text>
        <PasswordInput
          {...newPasswordField.getInputProps()}
          label="New Password"
          w="100%"
        />
        <PasswordInput
          {...confirmNewPasswordField.getInputProps()}
          label="Confirm Password"
          w="100%"
        />
        <Button
          onClick={() => doCreatePassword.mutate()}
          loading={doCreatePassword.isPending}
        >
          Submit
        </Button>
      </Stack>
    </Card>
  );
};

export default CreatePassword;
