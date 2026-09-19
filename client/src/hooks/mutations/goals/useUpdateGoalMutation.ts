import { NotificationType, showNotification } from "~/helpers/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { goalsQueryKey, translateAxiosError } from "~/helpers/requests";
import { IGoalUpdateRequest } from "~/models/goal";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useUpdateGoalMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (newGoal: IGoalUpdateRequest) =>
      await request({
        url: "/api/goal",
        method: "PUT",
        data: newGoal,
      }),
    onSuccess: async () => {
      queryClient.invalidateQueries({ queryKey: [goalsQueryKey] });
    },
    onError: (error: AxiosError) =>
      showNotification({
        type: NotificationType.Error,
        message: translateAxiosError(error),
      }),
  });
};
