import { Stack } from "@mantine/core";
import { BodyText, Modal } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import { ModalActions } from "~/components/ModalActions/ModalActions";
import type { INotificationDestinationResponse } from "~/models/notificationDestination";

interface DeleteNotificationDestinationModalProps {
  destination: INotificationDestinationResponse | null;
  isPending: boolean;
  onClose: () => void;
  onConfirm: (id: string) => void;
}

const DeleteNotificationDestinationModal = ({
  destination,
  isPending,
  onClose,
  onConfirm,
}: DeleteNotificationDestinationModalProps): React.ReactNode => {
  const { t } = useTranslation();

  return (
    <Modal
      opened={destination !== null}
      onClose={onClose}
      title={t("notification_delete_title")}
    >
      <Stack>
        <BodyText tone="secondary" size="sm">
          {t("notification_delete_confirm", { name: destination?.name })}
        </BodyText>
        <ModalActions
          actionColor="error"
          actionLabel={t("delete")}
          isPending={isPending}
          onAction={() => {
            if (destination) {
              onConfirm(destination.id);
            }
          }}
          onCancel={onClose}
        />
      </Stack>
    </Modal>
  );
};

export default DeleteNotificationDestinationModal;
