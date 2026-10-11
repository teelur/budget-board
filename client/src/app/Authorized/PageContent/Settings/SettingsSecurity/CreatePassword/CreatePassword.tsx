import { Stack } from "@mantine/core";
import {
  Button,
  Card,
  HeadingText,
  PasswordInput,
} from "@teelur/budget-board-ui";
import { hasLength, useField } from "@mantine/form";
import React from "react";
import { useTranslation } from "react-i18next";
import { useUpdatePasswordMutation } from "~/hooks/mutations/auth/useUpdatePasswordMutation";

const CreatePassword = (): React.ReactNode => {
  const { t } = useTranslation();
  const updatePasswordMutation = useUpdatePasswordMutation();

  const newPasswordField = useField<string>({
    initialValue: "",
    validate: hasLength(
      { min: 3 },
      t("password_min_length_message", { minLength: 3 }),
    ),
  });
  const confirmNewPasswordField = useField<string>({
    initialValue: "",
    validate: (value: string) =>
      value !== newPasswordField.getValue()
        ? t("passwords_do_not_match")
        : null,
  });

  return (
    <Card>
      <Stack gap="1rem">
        <HeadingText tone="heading" level={4}>
          {t("create_password")}
        </HeadingText>
        <Stack gap="0.5rem">
          <PasswordInput
            {...newPasswordField.getInputProps()}
            label={t("new_password")}
            w="100%"
          />
          <PasswordInput
            {...confirmNewPasswordField.getInputProps()}
            label={t("confirm_password")}
            w="100%"
          />
        </Stack>
        <Button
          variant="filled"
          color="primary"
          size="xs"
          onClick={() => {
            newPasswordField.validate();
            confirmNewPasswordField.validate();

            updatePasswordMutation.mutate({
              newPassword: newPasswordField.getValue(),
            });
          }}
          loading={updatePasswordMutation.isPending}
        >
          {t("create_password")}
        </Button>
      </Stack>
    </Card>
  );
};

export default CreatePassword;
