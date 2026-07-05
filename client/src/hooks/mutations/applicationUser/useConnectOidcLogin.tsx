import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { IOidcConnectRequest } from "~/models/oidc";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useConnectOidcLoginMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (oidcConnectRequest: IOidcConnectRequest) =>
      await request({
        url: "/api/applicationUser/connectOidcLogin",
        method: "POST",
        data: oidcConnectRequest,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["user"],
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
