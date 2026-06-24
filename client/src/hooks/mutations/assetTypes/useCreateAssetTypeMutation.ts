import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  assetsQueryKey,
  assetTypesQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { IAssetTypeCreateRequest } from "~/models/assetType";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useCreateAssetTypeMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (newAssetType: IAssetTypeCreateRequest) =>
      await request({
        url: "/api/assettype",
        method: "POST",
        data: newAssetType,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [assetTypesQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [assetsQueryKey] });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });
};
