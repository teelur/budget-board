import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  translateAxiosError,
  widgetSettingsQueryKey,
} from "~/helpers/requests";
import { INetWorthWidgetCategoryCreateRequest } from "~/models/netWorthWidgetConfiguration";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useCreateNetWorthWidgetCategoryMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (newCategory: INetWorthWidgetCategoryCreateRequest) =>
      await request({
        url: `/api/netWorthWidgetCategory`,
        method: "POST",
        data: newCategory,
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
