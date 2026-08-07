import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useLogoutMutation = () => {
  const queryClient = useQueryClient();
  const { request, setIsUserAuthenticated } = useAuth();

  return useMutation({
    mutationFn: async () =>
      await request({
        url: "/api/logout",
        method: "POST",
        data: {},
      }),
    onSuccess: async () => {
      queryClient.removeQueries();
      localStorage.setItem("isAuthenticated", "false");
      setIsUserAuthenticated(false);
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });
};
