import { Button, Stack } from "@mantine/core";
import { hasLength, useField } from "@mantine/form";
import React from "react";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import PasswordInput from "~/components/core/Input/PasswordInput/PasswordInput";
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
    <Card elevation={1}>
      <Stack gap="1rem">
        <PrimaryText size="lg">{t("create_password")}</PrimaryText>
        <PasswordInput
          {...newPasswordField.getInputProps()}
          label={<PrimaryText size="sm">{t("new_password")}</PrimaryText>}
          w="100%"
        />
        <PasswordInput
          {...confirmNewPasswordField.getInputProps()}
          label={<PrimaryText size="sm">{t("confirm_password")}</PrimaryText>}
          w="100%"
        />
        <Button
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
