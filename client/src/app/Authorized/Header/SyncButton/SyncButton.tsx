import { ActionIcon, Button, Tooltip } from "@mantine/core";
import { CloudSyncIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import { useSyncMutation } from "~/hooks/mutations/sync/useSyncMutation";

interface SyncButtonProps {
  compact?: boolean;
}

const SyncButton = ({ compact = false }: SyncButtonProps): React.ReactNode => {
  const { t } = useTranslation();
  const syncMutation = useSyncMutation();

  const syncLabel = t("sync");

  if (compact) {
    return (
      <Tooltip label={syncLabel}>
        <ActionIcon
          aria-label={syncLabel}
          loading={syncMutation.isPending}
          onClick={async () => await syncMutation.mutateAsync()}
          size="lg"
          variant="subtle"
        >
          <CloudSyncIcon size={20} />
        </ActionIcon>
      </Tooltip>
    );
  }

  return (
    <Button
      onClick={async () => await syncMutation.mutateAsync()}
      loading={syncMutation.isPending}
    >
      {syncLabel}
    </Button>
  );
};

export default SyncButton;
