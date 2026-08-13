import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  accountsQueryKey,
  accountTypesQueryKey,
  assetTypesQueryKey,
  assetsQueryKey,
  institutionsQueryKey,
  transactionCategoriesQueryKey,
  transactionsQueryKey,
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
      await queryClient.invalidateQueries({ queryKey: [transactionsQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [accountTypesQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [accountsQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [institutionsQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [assetTypesQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [assetsQueryKey] });
    },
    onError: (error: any) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });
};
