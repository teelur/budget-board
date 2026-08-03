import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  lunchFlowAccountQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

export const useUpdateSyncStartDateMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();
  const { dayjs } = useLocale();

  return useMutation({
    mutationFn: async (updateSyncStartDateRequest: {
      lunchFlowAccountGuid: string;
      syncStartDate: Date | null;
    }) =>
      await request({
        url: "/api/lunchFlowAccount/updateSyncStartDate",
        method: "PUT",
        params: {
          lunchFlowAccountGuid: updateSyncStartDateRequest.lunchFlowAccountGuid,
          syncStartDate: dayjs(
            updateSyncStartDateRequest.syncStartDate,
          ).isValid()
            ? dayjs(updateSyncStartDateRequest.syncStartDate).format(
                "YYYY-MM-DD",
              )
            : null,
        },
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [lunchFlowAccountQueryKey] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });
};
