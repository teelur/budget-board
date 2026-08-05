import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  applicationUserQueryKey,
  simpleFinAccountQueryKey,
  simpleFinOrganizationQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useUpdateAccessTokenMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (setupToken: string) =>
      await request({
        url: "/api/simpleFin/updateAccessToken",
        method: "PUT",
        params: { setupToken },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [applicationUserQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [simpleFinOrganizationQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [simpleFinAccountQueryKey],
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });
};
