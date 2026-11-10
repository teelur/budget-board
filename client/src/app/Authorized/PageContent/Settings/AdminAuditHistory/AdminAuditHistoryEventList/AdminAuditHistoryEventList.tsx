import { Stack } from "@mantine/core";
import { BodyText, Pagination, Skeleton } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import type { IAdminAuditEventResponse } from "~/models/adminAuditEvent";
import AdminAuditEventCard from "../AdminAuditEventCard/AdminAuditEventCard";

interface AdminAuditHistoryEventListProps {
  events: IAdminAuditEventResponse[];
  isPending: boolean;
  page: number;
  pageSize: number;
  totalCount: number;
  onPageChange: (page: number) => void;
}

const AdminAuditHistoryEventList = ({
  events,
  isPending,
  page,
  pageSize,
  totalCount,
  onPageChange,
}: AdminAuditHistoryEventListProps): React.ReactNode => {
  const { t } = useTranslation();

  if (isPending) {
    return <Skeleton height={240} />;
  }

  if (events.length === 0) {
    return <BodyText>{t("admin_history_no_events")}</BodyText>;
  }

  return (
    <Stack gap="0.5rem">
      <Stack gap="0.5rem">
        {events.map((event) => (
          <AdminAuditEventCard key={event.id} event={event} />
        ))}
      </Stack>
      <Stack justify="center" align="center" gap="0.25rem">
        <Pagination
          size="sm"
          value={page}
          onChange={onPageChange}
          total={Math.max(1, Math.ceil(totalCount / pageSize))}
        />
        <BodyText tone="secondary" size="xs">
          {t("admin_history_events_count", { count: totalCount })}
        </BodyText>
      </Stack>
    </Stack>
  );
};

export default AdminAuditHistoryEventList;
