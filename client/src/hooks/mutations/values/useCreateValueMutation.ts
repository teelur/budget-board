import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  assetsQueryKey,
  translateAxiosError,
  valuesQueryKey,
} from "~/helpers/requests";
import { IValueCreateRequest, IValueResponse } from "~/models/value";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

interface ICreateValueMutation {
  assetId: string;
}

export const useCreateValueMutation = ({ assetId }: ICreateValueMutation) => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (newValue: IValueCreateRequest) => {
      await request({
        url: `/api/value`,
        method: "POST",
        data: newValue,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [assetsQueryKey] });
      queryClient.invalidateQueries({
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
