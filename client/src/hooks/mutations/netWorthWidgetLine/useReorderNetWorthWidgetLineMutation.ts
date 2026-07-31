import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  translateAxiosError,
  widgetSettingsQueryKey,
} from "~/helpers/requests";
import { INetWorthWidgetLineReorderRequest } from "~/models/netWorthWidgetConfiguration";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useReorderNetWorthWidgetLineMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (reorderedLines: INetWorthWidgetLineReorderRequest) =>
      await request({
        url: `/api/netWorthWidgetLine/reorder`,
        method: "POST",
        data: reorderedLines,
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
