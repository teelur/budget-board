import { Group } from "@mantine/core";
import { Select, TextInput } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";

interface AdminAuditHistoryFiltersProps {
  actions: string[];
  actionFilter: string;
  resultFilter: string;
  userNameFilter: string;
  onUserNameChange: (value: string) => void;
  onActionChange: (value: string | null) => void;
  onResultChange: (value: string | null) => void;
}

const AdminAuditHistoryFilters = ({
  actions,
  actionFilter,
  resultFilter,
  userNameFilter,
  onUserNameChange,
  onActionChange,
  onResultChange,
}: AdminAuditHistoryFiltersProps): React.ReactNode => {
  const { t } = useTranslation();
  const actionOptions = [
    { value: "", label: t("all") },
    ...actions.map((action) => ({
      value: action,
      label: t(`admin_history_action_${action.replaceAll("-", "_")}`, {
        defaultValue: action,
      }),
    })),
  ];
  const resultOptions = [
    { value: "", label: t("admin_history_all_results") },
    { value: "success", label: t("admin_history_successful") },
    { value: "failure", label: t("admin_history_failed") },
  ];

  return (
    <Group align="flex-end" gap="0.5rem" wrap="wrap">
      <TextInput
        label={t("admin_history_user_filter")}
        placeholder={t("admin_history_user_filter_placeholder")}
        value={userNameFilter}
        onChange={(event) => onUserNameChange(event.currentTarget.value)}
        w={{ base: "100%", sm: "14rem" }}
      />
      <Select
        label={t("admin_history_action_filter")}
        data={actionOptions}
        value={actionFilter}
        onChange={onActionChange}
        w={{ base: "100%", sm: "14rem" }}
      />
      <Select
        label={t("admin_history_result_filter")}
        data={resultOptions}
        value={resultFilter}
        onChange={onResultChange}
        w={{ base: "100%", sm: "12rem" }}
      />
    </Group>
  );
};

export default AdminAuditHistoryFilters;
