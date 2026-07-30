import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  translateAxiosError,
  widgetSettingsQueryKey,
} from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useDeleteNetWorthWidgetCategoryMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (deleteRequest: {
      categoryId: string;
      lineId: string;
      widgetSettingsId: string;
    }) =>
      await request({
        url: `/api/netWorthWidgetCategory`,
        method: "DELETE",
        params: {
          categoryId: deleteRequest.categoryId,
          lineId: deleteRequest.lineId,
          widgetSettingsId: deleteRequest.widgetSettingsId,
        },
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
