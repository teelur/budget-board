import { Stack } from "@mantine/core";
import { isNotEmpty, useField } from "@mantine/form";
import {
  Button,
  Checkbox,
  Modal,
  MultiSelect,
  Select,
  Textarea,
  TextInput,
} from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import { NotificationType, showNotification } from "~/helpers/notifications";
import { useNotificationDestinationMutations } from "~/hooks/mutations/useNotificationDestinationMutations";
import type { INotificationDestinationDetailsResponse } from "~/models/notificationDestination";

interface NotificationDestinationFieldsProps {
  destination?: INotificationDestinationDetailsResponse;
  onClose: () => void;
}

const NotificationDestinationFields = ({
  destination,
  onClose,
}: NotificationDestinationFieldsProps): React.ReactNode => {
  const { t } = useTranslation();
  const { save } = useNotificationDestinationMutations();
  const name = useField<string>({
    initialValue: destination?.name ?? "",
    validate: isNotEmpty(t("notification_name_required")),
  });
  const url = useField<string>({
    initialValue: destination?.url ?? "",
    validate: destination
      ? undefined
      : isNotEmpty(t("notification_url_required")),
  });
  const headers = useField<string>({
    initialValue: destination
      ? JSON.stringify(destination.headers, null, 2)
      : "",
  });
  const contentType = useField<string>({
    initialValue: destination?.contentType ?? "application/json",
  });
  const body = useField<string>({
    initialValue: destination?.bodyTemplate ?? '{"content":"{{message}}"}',
  });
  const events = useField<string[]>({
    initialValue: destination?.eventTypes ?? [],
  });
  const includeDetails = useField<boolean>({
    initialValue: destination?.includeFinancialDetails ?? false,
  });

  const handleSave = async () => {
    if ((await name.validate()) || (await url.validate())) {
      return;
    }
    let parsedHeaders: Record<string, string> | undefined;
    if (headers.getValue().trim()) {
      try {
        const parsed: unknown = JSON.parse(headers.getValue());
        if (
          !parsed ||
          typeof parsed !== "object" ||
          Array.isArray(parsed) ||
          Object.values(parsed).some((value) => typeof value !== "string")
        ) {
          throw new Error("Invalid headers");
        }
        parsedHeaders = Object.fromEntries(
          Object.entries(parsed).map(([key, value]) => [key, String(value)]),
        );
      } catch {
        showNotification({
          type: NotificationType.Error,
          message: t("notification_headers_invalid"),
        });
        return;
      }
    }

    save.mutate(
      {
        id: destination?.id,
        data: {
          name: name.getValue().trim(),
          url: url.getValue().trim() || undefined,
          headers: parsedHeaders,
          contentType: contentType.getValue() || undefined,
          bodyTemplate: body.getValue() || undefined,
          eventTypes: events.getValue(),
          includeFinancialDetails: includeDetails.getValue(),
        },
      },
      { onSuccess: onClose },
    );
  };

  return (
    <Modal
      opened
      onClose={onClose}
      title={t(
        destination
          ? "notification_edit_destination"
          : "notification_add_destination",
      )}
    >
      <Stack gap="0.5rem">
        <TextInput label={t("name")} {...name.getInputProps()} />
        <TextInput
          label={t("notification_webhook_url")}
          {...url.getInputProps()}
        />
        <Textarea
          label={t("notification_headers")}
          description={t("notification_headers_description")}
          {...headers.getInputProps()}
        />
        <Select
          label={t("notification_content_type")}
          data={[
            { value: "application/json", label: "application/json" },
            { value: "text/plain", label: "text/plain" },
          ]}
          {...contentType.getInputProps()}
        />
        <Textarea
          label={t("notification_body_template")}
          description={t("notification_template_description")}
          {...body.getInputProps()}
        />
        <MultiSelect
          label={t("notification_events")}
          data={[
            { value: "sync_failed", label: t("notification_sync_failed") },
            { value: "import_failed", label: t("notification_import_failed") },
            { value: "goal_reached", label: t("notification_goal_reached") },
            { value: "budget_warning", label: t("notification_budget_warning") },
            {
              value: "transactions_added",
              label: t("notification_transactions_added"),
            },
            {
              value: "large_transactions_added",
              label: t("notification_large_transactions_added"),
            },
            {
              value: "recurring_reminder",
              label: t("notification_recurring_reminder"),
            },
          ]}
          {...events.getInputProps()}
        />
        <Checkbox
          label={t("notification_include_financial_details")}
          checked={includeDetails.getValue()}
          onChange={(event) =>
            includeDetails.setValue(event.currentTarget.checked)
          }
        />
        <Button
          variant="filled"
          color="primary"
          size="compact-sm"
          onClick={handleSave}
          loading={save.isPending}
        >
          {t("save")}
        </Button>
      </Stack>
    </Modal>
  );
};

export default NotificationDestinationFields;
