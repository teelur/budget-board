import { Group, Stack } from "@mantine/core";
import { BodyText, Button, NumberInput, Skeleton } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import { PlusIcon } from "lucide-react";
import { NotificationType, showNotification } from "~/helpers/notifications";
import { useUpdateUserSettingsMutation } from "~/hooks/mutations/userSettings/useUpdateUserSettingsMutation";
import { useNotificationDestinationMutations } from "~/hooks/mutations/useNotificationDestinationMutations";
import { useNotificationDestinationsQuery } from "~/hooks/queries/useNotificationDestinationsQuery";
import type { INotificationDestinationResponse } from "~/models/notificationDestination";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import DeleteNotificationDestinationModal from "./DeleteNotificationDestinationModal/DeleteNotificationDestinationModal";
import NotificationDestinationCard from "./NotificationDestinationCard/NotificationDestinationCard";
import NotificationDestinationForm from "./NotificationDestinationForm/NotificationDestinationForm";

const SettingsNotifications = (): React.ReactNode => {
  const { t } = useTranslation();
  const {
    largeTransactionThreshold,
    recurringReminderLeadTimeDays,
    decimalPlaces,
  } = useUserSettings();
  const destinations = useNotificationDestinationsQuery();
  const { remove, test } = useNotificationDestinationMutations();
  const updateUserSettingsMutation = useUpdateUserSettingsMutation();
  const [largeThresholdInput, setLargeThresholdInput] = React.useState<
    number | string
  >(largeTransactionThreshold ?? "");
  const [reminderLeadTimeInput, setReminderLeadTimeInput] = React.useState<
    number | string
  >(recurringReminderLeadTimeDays);
  const [form, setForm] = React.useState<
    INotificationDestinationResponse | "new" | null
  >(null);
  const [toDelete, setToDelete] =
    React.useState<INotificationDestinationResponse | null>(null);

  React.useEffect(() => {
    setLargeThresholdInput(largeTransactionThreshold ?? "");
  }, [largeTransactionThreshold]);

  React.useEffect(() => {
    setReminderLeadTimeInput(recurringReminderLeadTimeDays);
  }, [recurringReminderLeadTimeDays]);

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
      <Stack gap="xs">
        <BodyText tone="secondary" size="sm">
          {t("notification_preferences")}
        </BodyText>
        <NumberInput
          label={t("notification_large_transaction_threshold")}
          description={t("notification_large_transaction_threshold_description")}
          placeholder={t("notification_threshold_disabled")}
          min={0.01}
          decimalScale={decimalPlaces}
          allowNegative={false}
          value={largeThresholdInput}
          onChange={setLargeThresholdInput}
          onBlur={() => {
            const value =
              typeof largeThresholdInput === "number"
                ? largeThresholdInput
                : largeThresholdInput.trim() === ""
                  ? null
                  : Number(largeThresholdInput);
            if (
              value === null
              || (Number.isFinite(value) && value > 0)
            ) {
              if (value !== largeTransactionThreshold) {
                updateUserSettingsMutation.mutate({
                  largeTransactionThreshold: value,
                });
              }
            }
          }}
        />
        <NumberInput
          label={t("notification_recurring_reminder_days")}
          description={t("notification_recurring_reminder_days_description")}
          min={1}
          max={365}
          step={1}
          allowDecimal={false}
          allowNegative={false}
          value={reminderLeadTimeInput}
          onChange={setReminderLeadTimeInput}
          onBlur={() => {
            const value =
              typeof reminderLeadTimeInput === "number"
                ? reminderLeadTimeInput
                : Number(reminderLeadTimeInput);
            if (
              Number.isInteger(value)
              && value >= 1
              && value <= 365
              && value !== recurringReminderLeadTimeDays
            ) {
              updateUserSettingsMutation.mutate({
                recurringReminderLeadTimeDays: value,
              });
            }
          }}
        />
      </Stack>
      <Group justify="space-between" align="center" wrap="nowrap">
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
