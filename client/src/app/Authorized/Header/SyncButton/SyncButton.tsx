import { Button } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosResponse, AxiosError } from "axios";
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

const SyncButton = (): React.ReactNode => {
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

  return (
    <Button
      onClick={() => doSyncMutation.mutate()}
      loading={doSyncMutation.isPending}
    >
      {t("sync")}
    </Button>
  );
};

export default SyncButton;
