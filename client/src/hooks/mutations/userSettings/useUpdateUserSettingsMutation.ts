import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  transactionCategoriesQueryKey,
  translateAxiosError,
  userSettingsQueryKey,
} from "~/helpers/requests";
import { IUserSettingsUpdateRequest } from "~/models/userSettings";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useUpdateUserSettingsMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (updatedUserSettings: IUserSettingsUpdateRequest) =>
      await request({
        url: "/api/userSettings",
        method: "PUT",
        data: updatedUserSettings,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [userSettingsQueryKey] });
      await queryClient.invalidateQueries({
        queryKey: [transactionCategoriesQueryKey],
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
