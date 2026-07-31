import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  translateAxiosError,
  widgetSettingsQueryKey,
} from "~/helpers/requests";
import { INetWorthWidgetGroupReorderRequest } from "~/models/netWorthWidgetConfiguration";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useReorderNetWorthWidgetGroupMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (reorderRequest: INetWorthWidgetGroupReorderRequest) =>
      await request({
        url: `/api/netWorthWidgetGroup/reorder`,
        method: "POST",
        data: reorderRequest,
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
