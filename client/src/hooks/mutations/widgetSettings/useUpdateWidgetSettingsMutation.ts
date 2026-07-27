import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  translateAxiosError,
  widgetSettingsQueryKey,
} from "~/helpers/requests";
import { IWidgetSettingsUpdateRequest } from "~/models/widgetSettings";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useUpdateWidgetSettingsMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (updates: IWidgetSettingsUpdateRequest[]) =>
      await request({
        url: "/api/widgetSettings",
        method: "PUT",
        data: updates,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [widgetSettingsQueryKey],
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });
};
