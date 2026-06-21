import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { assetsQueryKey, translateAxiosError } from "~/helpers/requests";
import { IAssetUpdateRequest } from "~/models/asset";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useUpdateAssetMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (updatedAsset: IAssetUpdateRequest) =>
      await request({
        url: "/api/asset",
        method: "PUT",
        data: updatedAsset,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [assetsQueryKey] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        message: translateAxiosError(error),
        color: "var(--button-color-destructive)",
      });
    },
  });
};
