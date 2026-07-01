import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { budgetsQueryKey, translateAxiosError } from "~/helpers/requests";
import { IBudgetUpdateRequest } from "~/models/budget";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useUpdateBudgetMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (newBudget: IBudgetUpdateRequest) =>
      await request({
        url: "/api/budget",
        method: "PUT",
        data: newBudget,
      }),
    onSuccess: async () => {
      queryClient.invalidateQueries({ queryKey: [budgetsQueryKey] });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });
};
