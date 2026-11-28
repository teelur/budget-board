import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { AxiosError } from "axios";
import { NotificationType, showNotification } from "~/helpers/notifications";
import {
  notificationDestinationsQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import type { INotificationDestinationRequest } from "~/models/notificationDestination";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useNotificationDestinationMutations = () => {
  const { request } = useAuth();
  const queryClient = useQueryClient();
  const onError = (error: AxiosError) =>
    showNotification({
      type: NotificationType.Error,
      message: translateAxiosError(error),
    });
  const refresh = async () =>
    await queryClient.invalidateQueries({
      queryKey: [notificationDestinationsQueryKey],
    });

  const save = useMutation({
    mutationFn: async ({
      id,
      data,
    }: {
      id?: string;
      data: INotificationDestinationRequest;
    }) =>
      await request({
        url: id
          ? `/api/NotificationDestination/${id}`
          : "/api/NotificationDestination",
        method: id ? "PUT" : "POST",
        data,
      }),
    onSuccess: refresh,
    onError,
  });
  const remove = useMutation({
    mutationFn: async (id: string) =>
      await request({
        url: `/api/NotificationDestination/${id}`,
        method: "DELETE",
      }),
    onSuccess: refresh,
    onError,
  });
  const test = useMutation({
    mutationFn: async (id: string) =>
      await request({
        url: `/api/NotificationDestination/${id}/test`,
        method: "POST",
      }),
    onError,
  });
  const setEnabled = useMutation({
    mutationFn: async ({ id, enabled }: { id: string; enabled: boolean }) =>
      await request({
        url: `/api/NotificationDestination/${id}/enabled`,
        method: "PATCH",
        data: { enabled },
      }),
    onSuccess: refresh,
    onError,
  });
  return { save, remove, test, setEnabled };
};
