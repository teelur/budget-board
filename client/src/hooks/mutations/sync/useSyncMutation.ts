import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { useTranslation } from "react-i18next";
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
import { SyncError } from "~/models/sync";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useSyncMutation = () => {
  const { request } = useAuth();
  const queryClient = useQueryClient();
  const { t } = useTranslation();

  return useMutation({
    mutationFn: async () => await request({ url: "/api/sync", method: "GET" }),
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
};
