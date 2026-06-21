import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { assetsQueryKey, translateAxiosError } from "~/helpers/requests";
import { IAssetIndexRequest } from "~/models/asset";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useOrderAssetsMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (orderedAssets: IAssetIndexRequest[]) =>
      await request({
        url: "/api/asset/order",
        method: "PUT",
        data: orderedAssets,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [assetsQueryKey] });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });
};
