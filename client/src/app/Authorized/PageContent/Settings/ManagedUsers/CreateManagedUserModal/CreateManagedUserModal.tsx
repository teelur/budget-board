import { Stack } from "@mantine/core";
import { isNotEmpty, useField } from "@mantine/form";
import { BodyText, Button, Modal, TextInput } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import { useCreateManagedUserMutation } from "~/hooks/mutations/useManagedUserMutations";
import type { IManagedUserOperationResponse } from "~/models/managedUser";

interface CreateManagedUserModalProps {
  opened: boolean;
  onClose: () => void;
  onTemporaryPassword: (response: IManagedUserOperationResponse) => void;
}

const CreateManagedUserModal = ({
  opened,
  onClose,
  onTemporaryPassword,
}: CreateManagedUserModalProps): React.ReactNode => {
  const { t } = useTranslation();
  const createUserMutation = useCreateManagedUserMutation();
  const usernameField = useField<string>({
    initialValue: "",
    validate: isNotEmpty(t("username_required_message")),
  });
  const emailField = useField<string>({ initialValue: "" });

  const handleCreateUser = async () => {
    if (await usernameField.validate()) {
      return;
    }

    createUserMutation.mutate(
      {
        username: usernameField.getValue().trim(),
        email: emailField.getValue().trim() || null,
      },
      {
        onSuccess: (response) => {
          usernameField.reset();
          emailField.reset();
          onClose();
          onTemporaryPassword(response);
        },
      },
    );
  };

  return (
    <Modal opened={opened} onClose={onClose} title={t("create_managed_user")}>
      <Stack gap="0.5rem">
        <BodyText tone="secondary" size="sm">
          {t("temporary_password_notice")}
        </BodyText>
        <TextInput
          {...usernameField.getInputProps()}
          label={t("username")}
          autoComplete="off"
        />
        <TextInput
          {...emailField.getInputProps()}
          label={t("email")}
          type="email"
          autoComplete="off"
        />
        <Button
          variant="filled"
          color="primary"
          size="compact-sm"
          loading={createUserMutation.isPending}
          onClick={handleCreateUser}
        >
          {t("submit")}
        </Button>
      </Stack>
    </Modal>
  );
};

export default CreateManagedUserModal;
