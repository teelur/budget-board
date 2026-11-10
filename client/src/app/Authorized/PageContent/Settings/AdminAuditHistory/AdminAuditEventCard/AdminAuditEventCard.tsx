import { Group, Stack } from "@mantine/core";
import { Badge, BodyText, Card } from "@teelur/budget-board-ui";
import { ArrowRight } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import type { IAdminAuditEventResponse } from "~/models/adminAuditEvent";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface AdminAuditEventCardProps {
  event: IAdminAuditEventResponse;
}

const AdminAuditEventCard = ({
  event,
}: AdminAuditEventCardProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs, dateFormat } = useLocale();

  return (
    <Card>
      <Card.Section>
        <Group justify="space-between">
          <Stack gap={0}>
            <BodyText
              tone="primary"
              size="sm"
              fw={600}
              style={{ overflowWrap: "anywhere" }}
            >
              {t(`admin_history_action_${event.action.replaceAll("-", "_")}`, {
                defaultValue: event.action,
              })}
            </BodyText>
            {event.details && (
              <Group gap="0.25rem" wrap="wrap">
                <BodyText
                  tone="secondary"
                  size="xs"
                  style={{ overflowWrap: "anywhere" }}
                >
                  {t("admin_history_details")}:
                </BodyText>
                <BodyText
                  tone="primary"
                  size="xs"
                  style={{ overflowWrap: "anywhere" }}
                >
                  {event.details}
                </BodyText>
              </Group>
            )}
          </Stack>
          <Stack align="center" gap={0} flex={1}>
            <Group gap="0.25rem" wrap="wrap">
              <Group gap="0.125rem" wrap="nowrap">
                <BodyText tone="secondary" size="xs">
                  {t("admin_history_actor")}:
                </BodyText>
                <BodyText size="sm" style={{ overflowWrap: "anywhere" }}>
                  {event.actorUserName ??
                    event.actorUserId ??
                    t("admin_history_system")}
                </BodyText>
              </Group>
              <ArrowRight size={14} aria-hidden="true" />
              <Group gap="0.125rem" wrap="nowrap">
                <BodyText tone="secondary" size="xs">
                  {t("admin_history_target")}:
                </BodyText>
                <BodyText size="sm" style={{ overflowWrap: "anywhere" }}>
                  {event.targetUserName ?? event.targetUserId ?? t("none")}
                </BodyText>
              </Group>
            </Group>
            {event.requestIpAddress && (
              <Group gap="0.25rem" wrap="wrap">
                <BodyText tone="secondary" size="xs">
                  {t("admin_history_ip_address")}:
                </BodyText>
                <BodyText tone="primary" size="xs">
                  {event.requestIpAddress}
                </BodyText>
              </Group>
            )}
          </Stack>
          <Stack gap="0.25rem">
            <Badge
              variant="filled"
              size="xs"
              color={event.succeeded ? "success" : "error"}
              w="min-content"
            >
              {event.succeeded
                ? t("admin_history_successful")
                : t("admin_history_failed")}
            </Badge>
            <BodyText tone="secondary" size="xs">
              {dayjs(event.createdAt).format(`${dateFormat} LT`)}
            </BodyText>
          </Stack>
        </Group>
      </Card.Section>
    </Card>
  );
};

export default AdminAuditEventCard;
