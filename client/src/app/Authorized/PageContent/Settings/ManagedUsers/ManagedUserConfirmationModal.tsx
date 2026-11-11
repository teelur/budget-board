import { Group, Stack } from "@mantine/core";
import { BodyText, Button, Modal } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import type { IManagedUserResponse } from "~/models/managedUser";

type ManagedUserAction = "reset_password" | "disable" | "transfer_superuser";

export interface ManagedUserConfirmation {
  action: ManagedUserAction;
  user: IManagedUserResponse;
}

interface ManagedUserConfirmationModalProps {
  confirmation: ManagedUserConfirmation | null;
  isPending: boolean;
  onClose: () => void;
  onConfirm: () => void;
}

const ManagedUserConfirmationModal = ({
  confirmation,
  isPending,
  onClose,
  onConfirm,
}: ManagedUserConfirmationModalProps): React.ReactNode => {
  const { t } = useTranslation();
  const confirmationMessage =
    confirmation?.action === "reset_password"
      ? t("confirm_managed_user_password_reset", {
          username: confirmation.user.username,
        })
      : confirmation?.action === "disable"
        ? t("confirm_disable_managed_user", {
            username: confirmation.user.username,
          })
        : confirmation
          ? t("confirm_superuser_transfer", {
              username: confirmation.user.username,
            })
          : "";

  const confirmationTitle =
    confirmation?.action === "reset_password"
      ? t("reset_password")
      : confirmation?.action === "disable"
        ? t("disable_account")
        : t("transfer_superuser");

  return (
    <Modal
      opened={confirmation !== null}
      onClose={onClose}
      title={confirmationTitle}
    >
      <Stack gap="sm">
        <BodyText tone="secondary" size="sm">
          {confirmationMessage}
        </BodyText>
        <Group gap="0.5rem">
          <Button
            variant="filled"
            color="neutral"
            size="xs"
            flex="1 1 auto"
            disabled={isPending}
            onClick={onClose}
          >
            {t("cancel")}
          </Button>
          <Button
            variant="filled"
            color="error"
            size="xs"
            flex="1 1 auto"
            loading={isPending}
            onClick={onConfirm}
          >
            {t("confirm_action")}
          </Button>
        </Group>
      </Stack>
    </Modal>
  );
};

export default ManagedUserConfirmationModal;
