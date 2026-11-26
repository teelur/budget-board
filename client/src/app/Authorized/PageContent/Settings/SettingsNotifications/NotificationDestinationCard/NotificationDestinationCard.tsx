import { Group } from "@mantine/core";
import { BodyText, Button, Card } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import type { INotificationDestinationResponse } from "~/models/notificationDestination";

interface NotificationDestinationCardProps {
  destination: INotificationDestinationResponse;
  isTestPending: boolean;
  onTest: () => void;
  onEdit: () => void;
  onDelete: () => void;
}

const NotificationDestinationCard = ({
  destination,
  isTestPending,
  onTest,
  onEdit,
  onDelete,
}: NotificationDestinationCardProps): React.ReactNode => {
  const { t } = useTranslation();

  return (
    <Card>
      <Group justify="space-between">
        <BodyText tone="primary" size="md">
          {destination.name}
        </BodyText>
        <Group gap="0.5rem">
          <Button
            size="compact-sm"
            variant="ghost"
            onClick={onTest}
            loading={isTestPending}
          >
            {t("notification_send_test")}
          </Button>
          <Button size="compact-sm" variant="ghost" onClick={onEdit}>
            {t("edit")}
          </Button>
          <Button size="compact-sm" variant="ghost" onClick={onDelete}>
            {t("delete")}
          </Button>
        </Group>
      </Group>
    </Card>
  );
};

export default NotificationDestinationCard;
