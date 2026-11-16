import { Stack } from "@mantine/core";
import { isNotEmpty, useField } from "@mantine/form";
import { Button, Modal, TextInput } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import { useUpdateManagedUserUsernameMutation } from "~/hooks/mutations/useManagedUserMutations";
import type { IManagedUserResponse } from "~/models/managedUser";

interface EditManagedUserUsernameModalProps {
  opened: boolean;
  user: IManagedUserResponse;
  onClose: () => void;
}

const EditManagedUserUsernameModal = ({
  opened,
  user,
  onClose,
}: EditManagedUserUsernameModalProps): React.ReactNode => {
  const { t } = useTranslation();
  const updateUsernameMutation = useUpdateManagedUserUsernameMutation();
  const usernameField = useField<string>({
    initialValue: user.username,
    validate: isNotEmpty(t("username_required_message")),
  });

  const handleUpdateUsername = async () => {
    if (await usernameField.validate()) {
      return;
    }

    updateUsernameMutation.mutate(
      {
        userId: user.id,
        username: usernameField.getValue().trim(),
      },
      {
        onSuccess: () => {
          usernameField.reset();
          onClose();
        },
      },
    );
  };

  return (
    <Modal
      opened={opened}
      onClose={onClose}
      title={t("edit_managed_user_username")}
    >
      <Stack gap="0.5rem">
        <TextInput
          {...usernameField.getInputProps()}
          label={t("username")}
          maxLength={256}
          autoComplete="off"
        />
        <Button
          variant="filled"
          color="primary"
          size="compact-sm"
          loading={updateUsernameMutation.isPending}
          onClick={handleUpdateUsername}
        >
          {t("submit")}
        </Button>
      </Stack>
    </Modal>
  );
};

export default EditManagedUserUsernameModal;
