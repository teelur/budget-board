import { Stack } from "@mantine/core";
import { useDebouncedValue } from "@mantine/hooks";
import { BodyText, Skeleton } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import { Navigate } from "react-router";
import { useAdminAuditEventsQuery } from "~/hooks/queries/useAdminAuditEventsQuery";
import { useApplicationUserQuery } from "~/hooks/queries/useApplicationUserQuery";
import AdminAuditHistoryEventList from "./AdminAuditHistoryEventList/AdminAuditHistoryEventList";
import AdminAuditHistoryFilters from "./AdminAuditHistoryFilters/AdminAuditHistoryFilters";

const pageSize = 10;

const AdminAuditHistory = (): React.ReactNode => {
  const { t } = useTranslation();
  const applicationUserQuery = useApplicationUserQuery();
  const isSuperuser = applicationUserQuery.data?.isSuperuser === true;
  const [page, setPage] = React.useState(1);
  const [actionFilter, setActionFilter] = React.useState("");
  const [resultFilter, setResultFilter] = React.useState("");
  const [userNameFilter, setUserNameFilter] = React.useState("");
  const [debouncedUserNameFilter] = useDebouncedValue(
    userNameFilter.trim(),
    300,
  );
  const auditEventsQuery = useAdminAuditEventsQuery({
    enabled:
      isSuperuser &&
      !applicationUserQuery.isPending &&
      !applicationUserQuery.isError,
    page,
    pageSize,
    action: actionFilter || undefined,
    succeeded:
      resultFilter === "success"
        ? true
        : resultFilter === "failure"
          ? false
          : undefined,
    userName: debouncedUserNameFilter || undefined,
  });

  if (applicationUserQuery.isPending) {
    return <Skeleton height={300} />;
  }

  if (applicationUserQuery.isError) {
    return <BodyText>{t("admin_history_access_check_error")}</BodyText>;
  }

  if (!isSuperuser) {
    return <Navigate to="/settings/user" replace />;
  }

  if (auditEventsQuery.isError) {
    return <BodyText>{t("admin_history_query_error")}</BodyText>;
  }

  return (
    <Stack gap="0.5rem">
      <BodyText tone="secondary" size="sm">
        {t("admin_history_description")}
      </BodyText>
      <AdminAuditHistoryFilters
        actions={auditEventsQuery.data?.actions ?? []}
        actionFilter={actionFilter}
        resultFilter={resultFilter}
        userNameFilter={userNameFilter}
        onUserNameChange={(value) => {
          setPage(1);
          setUserNameFilter(value);
        }}
        onActionChange={(value) => {
          setPage(1);
          setActionFilter(value ?? "");
        }}
        onResultChange={(value) => {
          setPage(1);
          setResultFilter(value ?? "");
        }}
      />
      <AdminAuditHistoryEventList
        isPending={auditEventsQuery.isPending}
        events={auditEventsQuery.data?.events ?? []}
        totalCount={auditEventsQuery.data?.totalCount ?? 0}
        page={page}
        pageSize={pageSize}
        onPageChange={setPage}
      />
    </Stack>
  );
};

export default AdminAuditHistory;
