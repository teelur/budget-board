import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { goalsQueryKey, translateAxiosError } from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useCompleteGoalMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (goalId: string) =>
      await request({
        url: "/api/goal/complete",
        method: "POST",
        params: { goalId },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [goalsQueryKey],
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
