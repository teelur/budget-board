import { ActionIcon, Button, Tooltip } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosResponse, AxiosError } from "axios";
import { CloudSyncIcon } from "lucide-react";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import {
  accountsQueryKey,
  goalsQueryKey,
  institutionsQueryKey,
  lunchFlowAccountQueryKey,
  simpleFinAccountQueryKey,
  simpleFinOrganizationQueryKey,
  transactionsQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { useTranslation } from "react-i18next";
import { SyncError } from "~/models/sync";

interface SyncButtonProps {
  compact?: boolean;
}

const SyncButton = ({ compact = false }: SyncButtonProps): React.ReactNode => {
  const { t } = useTranslation();

  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doSyncMutation = useMutation({
    mutationFn: async () =>
      await request({ url: "/api/simplefin/sync", method: "GET" }),
    onSuccess: async (data: AxiosResponse) => {
      await queryClient.invalidateQueries({ queryKey: [transactionsQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [institutionsQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [accountsQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [goalsQueryKey] });
      await queryClient.invalidateQueries({
        queryKey: [simpleFinOrganizationQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [simpleFinAccountQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [lunchFlowAccountQueryKey],
      });
      if ((data.data?.length ?? 0) > 0) {
        {
          data.data.map((error: SyncError) =>
            notifications.show({
              color: "var(--button-color-destructive)",
              title: t("syncErrorFromSource", { source: error.source }),
              message: error.message,
            }),
          );
        }
      }
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  const syncLabel = t("sync");

  if (compact) {
    return (
      <Tooltip label={syncLabel}>
        <ActionIcon
          aria-label={syncLabel}
          loading={doSyncMutation.isPending}
          onClick={() => doSyncMutation.mutate()}
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
      onClick={() => doSyncMutation.mutate()}
      loading={doSyncMutation.isPending}
    >
      {syncLabel}
    </Button>
  );
};

export default SyncButton;
