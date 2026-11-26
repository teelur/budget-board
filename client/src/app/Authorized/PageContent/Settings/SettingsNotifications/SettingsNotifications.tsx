import { Group, Stack } from "@mantine/core";
import { BodyText, Button, Skeleton } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import { PlusIcon } from "lucide-react";
import { NotificationType, showNotification } from "~/helpers/notifications";
import { useNotificationDestinationMutations } from "~/hooks/mutations/useNotificationDestinationMutations";
import { useNotificationDestinationsQuery } from "~/hooks/queries/useNotificationDestinationsQuery";
import type { INotificationDestinationResponse } from "~/models/notificationDestination";
import DeleteNotificationDestinationModal from "./DeleteNotificationDestinationModal/DeleteNotificationDestinationModal";
import NotificationDestinationCard from "./NotificationDestinationCard/NotificationDestinationCard";
import NotificationDestinationForm from "./NotificationDestinationForm/NotificationDestinationForm";

const SettingsNotifications = (): React.ReactNode => {
  const { t } = useTranslation();
  const destinations = useNotificationDestinationsQuery();
  const { remove, test } = useNotificationDestinationMutations();
  const [form, setForm] = React.useState<
    INotificationDestinationResponse | "new" | null
  >(null);
  const [toDelete, setToDelete] =
    React.useState<INotificationDestinationResponse | null>(null);

  if (destinations.isPending) {
    return <Skeleton height={150} />;
  }
  if (destinations.isError) {
    return (
      <BodyText tone="muted" size="sm" ta="center">
        {t("notification_load_error")}
      </BodyText>
    );
  }

  return (
    <Stack gap="sm">
      <Group justify="space-between" align="center">
        <BodyText tone="secondary" size="sm">
          {t("notification_settings_description")}
        </BodyText>
        <Button
          variant="filled"
          color="primary"
          size="compact-sm"
          leftSection={<PlusIcon size="1rem" />}
          onClick={() => setForm("new")}
        >
          {t("notification_add_destination")}
        </Button>
      </Group>
      {destinations.data.length === 0 && (
        <BodyText tone="muted" size="sm" ta="center">
          {t("notification_no_destinations")}
        </BodyText>
      )}
      {destinations.data.map((destination) => (
        <NotificationDestinationCard
          key={destination.id}
          destination={destination}
          isTestPending={test.isPending}
          onTest={() =>
            test.mutate(destination.id, {
              onSuccess: () =>
                showNotification({
                  type: NotificationType.Success,
                  message: t("notification_test_sent"),
                }),
            })
          }
          onEdit={() => setForm(destination)}
          onDelete={() => setToDelete(destination)}
        />
      ))}
      {form && (
        <NotificationDestinationForm
          destination={form === "new" ? undefined : form}
          onClose={() => setForm(null)}
        />
      )}
      <DeleteNotificationDestinationModal
        destination={toDelete}
        isPending={remove.isPending}
        onClose={() => setToDelete(null)}
        onConfirm={(id) =>
          remove.mutate(id, {
            onSuccess: () => setToDelete(null),
          })
        }
      />
    </Stack>
  );
};

export default SettingsNotifications;
