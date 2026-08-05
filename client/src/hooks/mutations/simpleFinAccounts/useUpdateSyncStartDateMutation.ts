import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  simpleFinAccountQueryKey,
  simpleFinOrganizationQueryKey,
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
      simpleFinAccountGuid: string;
      syncStartDate: Date | null;
    }) =>
      await request({
        url: "/api/simpleFinAccount/updateSyncStartDate",
        method: "PUT",
        params: {
          simpleFinAccountGuid: updateSyncStartDateRequest.simpleFinAccountGuid,
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
      queryClient.invalidateQueries({ queryKey: [simpleFinAccountQueryKey] });
      queryClient.invalidateQueries({
        queryKey: [simpleFinOrganizationQueryKey],
      });
    },
    onError: (error: any) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });
};
