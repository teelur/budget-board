import { Stack } from "@mantine/core";
import { BodyText, Modal } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import { ModalActions } from "~/components/ModalActions/ModalActions";
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
        <ModalActions
          actionColor="error"
          actionLabel={t("confirm_action")}
          isPending={isPending}
          onAction={onConfirm}
          onCancel={onClose}
        />
      </Stack>
    </Modal>
  );
};

export default ManagedUserConfirmationModal;
