import { NotificationType, showNotification } from "~/helpers/notifications";
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
    mutationFn: async (valueId: string) =>
      await request({
        url: `/api/value`,
        method: "DELETE",
        params: { valueId },
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
      showNotification({
        type: NotificationType.Error,
        message: translateAxiosError(error),
      }),
  });
};
