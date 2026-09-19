import { NotificationType, showNotification } from "~/helpers/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { goalsQueryKey, translateAxiosError } from "~/helpers/requests";
import { IGoalCreateRequest } from "~/models/goal";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useCreateGoalMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (newGoal: IGoalCreateRequest) =>
      await request({
        url: "/api/goal",
        method: "POST",
        data: newGoal,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [goalsQueryKey] });
    },
    onError: (error: any) => {
      showNotification({
        type: NotificationType.Error,
        message: translateAxiosError(error),
      });
    },
  });
};
