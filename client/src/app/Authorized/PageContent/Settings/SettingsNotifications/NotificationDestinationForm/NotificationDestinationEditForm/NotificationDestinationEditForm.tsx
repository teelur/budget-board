import { Stack } from "@mantine/core";
import { BodyText, Button, Modal, Skeleton } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import { useNotificationDestinationDetailsQuery } from "~/hooks/queries/useNotificationDestinationDetailsQuery";
import type { INotificationDestinationResponse } from "~/models/notificationDestination";
import NotificationDestinationFields from "../../NotificationDestinationFields/NotificationDestinationFields";

interface NotificationDestinationEditFormProps {
  destination: INotificationDestinationResponse;
  onClose: () => void;
}

const NotificationDestinationEditForm = ({
  destination,
  onClose,
}: NotificationDestinationEditFormProps): React.ReactNode => {
  const { t } = useTranslation();
  const detailsQuery = useNotificationDestinationDetailsQuery(destination.id);

  if (detailsQuery.isPending) {
    return (
      <Modal
        opened
        onClose={onClose}
        title={t("notification_edit_destination")}
      >
        <Skeleton height={280} />
      </Modal>
    );
  }

  if (detailsQuery.isError || !detailsQuery.data) {
    return (
      <Modal
        opened
        onClose={onClose}
        title={t("notification_edit_destination")}
      >
        <Stack gap="0.5rem">
          <BodyText tone="muted" size="sm" ta="center">
            {t("notification_load_error")}
          </BodyText>
          <Button
            variant="filled"
            color="primary"
            size="compact-sm"
            onClick={onClose}
          >
            {t("cancel")}
          </Button>
        </Stack>
      </Modal>
    );
  }

  return (
    <NotificationDestinationFields
      destination={detailsQuery.data}
      onClose={onClose}
    />
  );
};

export default NotificationDestinationEditForm;
