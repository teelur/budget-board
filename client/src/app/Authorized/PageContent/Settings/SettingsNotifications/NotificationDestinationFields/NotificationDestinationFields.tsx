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
import type {
  INotificationDestinationDetailsResponse,
  INotificationDestinationRequest,
  NotificationDestinationProvider,
} from "~/models/notificationDestination";

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
  const provider = useField<NotificationDestinationProvider>({
    initialValue: destination?.provider ?? "webhook",
  });
  const name = useField<string>({
    initialValue: destination?.name ?? "",
    validate: isNotEmpty(t("notification_name_required")),
  });
  const webhookDestination =
    destination?.provider === "webhook" ? destination : undefined;
  const ntfyDestination =
    destination?.provider === "ntfy" ? destination : undefined;
  const discordDestination =
    destination?.provider === "discord" ? destination : undefined;
  const url = useField<string>({
    initialValue: webhookDestination?.url ?? "",
    validate: isNotEmpty(t("notification_url_required")),
  });
  const headers = useField<string>({
    initialValue: webhookDestination
      ? JSON.stringify(webhookDestination.headers, null, 2)
      : "",
  });
  const contentType = useField<string>({
    initialValue: webhookDestination?.contentType ?? "application/json",
  });
  const body = useField<string>({
    initialValue: webhookDestination?.bodyTemplate ?? '{"content":"{{message}}"}',
  });
  const ntfyServerUrl = useField<string>({
    initialValue: ntfyDestination?.ntfyServerUrl ?? "https://ntfy.sh",
    validate: isNotEmpty(t("notification_ntfy_server_url_required")),
  });
  const ntfyTopic = useField<string>({
    initialValue: ntfyDestination?.ntfyTopic ?? "",
    validate: isNotEmpty(t("notification_ntfy_topic_required")),
  });
  const ntfyAccessToken = useField<string>({
    initialValue: ntfyDestination?.ntfyAccessToken ?? "",
  });
  const discordWebhookUrl = useField<string>({
    initialValue: discordDestination?.discordWebhookUrl ?? "",
    validate: isNotEmpty(t("notification_discord_webhook_url_required")),
  });
  const events = useField<string[]>({
    initialValue: destination?.eventTypes ?? [],
  });
  const includeDetails = useField<boolean>({
    initialValue: destination?.includeFinancialDetails ?? false,
  });

  const saveDestination = (data: INotificationDestinationRequest) => {
    save.mutate(
      { id: destination?.id, data },
      { onSuccess: onClose },
    );
  };

  const handleSave = async () => {
    if (await name.validate()) {
      return;
    }

    const common = {
      name: name.getValue().trim(),
      eventTypes: events.getValue(),
      includeFinancialDetails: includeDetails.getValue(),
    };

    switch (provider.getValue()) {
      case "webhook": {
        if (await url.validate()) {
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

        saveDestination({
          ...common,
          provider: "webhook",
          url: url.getValue().trim(),
          headers: parsedHeaders,
          contentType: contentType.getValue(),
          bodyTemplate: body.getValue(),
        });
        return;
      }
      case "ntfy":
        if (
          (await ntfyServerUrl.validate()) ||
          (await ntfyTopic.validate())
        ) {
          return;
        }
        saveDestination({
          ...common,
          provider: "ntfy",
          ntfyServerUrl: ntfyServerUrl.getValue().trim(),
          ntfyTopic: ntfyTopic.getValue().trim(),
          ntfyAccessToken: ntfyAccessToken.getValue(),
        });
        return;
      case "discord":
        if (await discordWebhookUrl.validate()) {
          return;
        }
        saveDestination({
          ...common,
          provider: "discord",
          discordWebhookUrl: discordWebhookUrl.getValue().trim(),
        });
    }
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
        <Select
          label={t("notification_provider")}
          data={[
            { value: "webhook", label: t("notification_provider_webhook") },
            { value: "ntfy", label: t("notification_provider_ntfy") },
            { value: "discord", label: t("notification_provider_discord") },
          ]}
          disabled={Boolean(destination)}
          {...provider.getInputProps()}
        />
        {provider.getValue() === "webhook" && (
          <>
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
          </>
        )}
        {provider.getValue() === "ntfy" && (
          <>
            <TextInput
              label={t("notification_ntfy_server_url")}
              {...ntfyServerUrl.getInputProps()}
            />
            <TextInput
              label={t("notification_ntfy_topic")}
              {...ntfyTopic.getInputProps()}
            />
            <TextInput
              type="password"
              autoComplete="new-password"
              label={t("notification_ntfy_access_token")}
              description={t("notification_ntfy_access_token_description")}
              {...ntfyAccessToken.getInputProps()}
            />
          </>
        )}
        {provider.getValue() === "discord" && (
          <TextInput
            type="password"
            autoComplete="new-password"
            label={t("notification_discord_webhook_url")}
            {...discordWebhookUrl.getInputProps()}
          />
        )}
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
