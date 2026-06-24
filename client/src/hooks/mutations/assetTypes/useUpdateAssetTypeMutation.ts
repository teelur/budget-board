import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  assetsQueryKey,
  assetTypesQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { IAssetTypeUpdateRequest } from "~/models/assetType";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useUpdateAssetTypeMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (req: IAssetTypeUpdateRequest) =>
      await request({
        url: "/api/assetType",
        method: "PUT",
        data: req,
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
