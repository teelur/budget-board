import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  assetsQueryKey,
  valuesQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

interface UseDeleteValueMutationProps {
  assetId: string;
}

export const useDeleteValueMutation = ({
  assetId,
}: UseDeleteValueMutationProps) => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async () =>
      await request({
        url: `/api/value`,
        method: "DELETE",
        params: { guid: assetId },
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
