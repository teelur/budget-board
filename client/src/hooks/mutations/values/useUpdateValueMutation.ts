import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  assetsQueryKey,
  translateAxiosError,
  valuesQueryKey,
} from "~/helpers/requests";
import { IValueUpdateRequest } from "~/models/value";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

interface IUpdateValueMutation {
  assetId: string;
}

export const useUpdateValueMutation = ({ assetId }: IUpdateValueMutation) => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (updatedValue: IValueUpdateRequest) =>
      await request({
        url: `/api/value`,
        method: "PUT",
        data: updatedValue,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [assetsQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [valuesQueryKey, assetId],
      });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });
};
