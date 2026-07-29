import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  translateAxiosError,
  widgetSettingsQueryKey,
} from "~/helpers/requests";
import { INetWorthWidgetLineCreateRequest } from "~/models/netWorthWidgetConfiguration";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useCreateNetWorthWidgetLineMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (newLine: INetWorthWidgetLineCreateRequest) =>
      await request({
        url: `/api/netWorthWidgetLine`,
        method: "POST",
        data: newLine,
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
