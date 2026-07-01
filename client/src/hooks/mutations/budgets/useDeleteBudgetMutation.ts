import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { budgetsQueryKey, translateAxiosError } from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useDeleteBudgetMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (id: string) =>
      await request({
        url: "/api/budget",
        method: "DELETE",
        params: { guid: id },
      }),
    onSuccess: async () =>
      await queryClient.invalidateQueries({ queryKey: [budgetsQueryKey] }),
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });
};
