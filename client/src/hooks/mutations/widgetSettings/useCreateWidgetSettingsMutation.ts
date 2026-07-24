import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  translateAxiosError,
  widgetSettingsQueryKey,
} from "~/helpers/requests";
import { IWidgetSettingsCreateRequest } from "~/models/widgetSettings";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useCreateWidgetSettingsMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (newWidgetSettings: IWidgetSettingsCreateRequest) =>
      await request({
        url: "/api/widgetSettings",
        method: "POST",
        data: newWidgetSettings,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [widgetSettingsQueryKey] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });
};
