import { Button } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosResponse, AxiosError } from "axios";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import {
  simpleFinAccountQueryKey,
  simpleFinOrganizationQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { useTranslation } from "react-i18next";

const SyncButton = (): React.ReactNode => {
  const { t } = useTranslation();

  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doSyncMutation = useMutation({
    mutationFn: async () =>
      await request({ url: "/api/simplefin/sync", method: "GET" }),
    onSuccess: async (data: AxiosResponse) => {
      await queryClient.invalidateQueries({ queryKey: ["transactions"] });
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });
      await queryClient.invalidateQueries({ queryKey: ["goals"] });
      await queryClient.invalidateQueries({
        queryKey: [simpleFinOrganizationQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [simpleFinAccountQueryKey],
      });
      if ((data.data?.length ?? 0) > 0) {
        {
          data.data.map((error: string) =>
            notifications.show({
              color: "var(--button-color-destructive)",
              message: error,
            })
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
