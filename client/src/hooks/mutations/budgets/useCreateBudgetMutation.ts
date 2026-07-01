import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { budgetsQueryKey, translateAxiosError } from "~/helpers/requests";
import { IBudgetCreateRequest } from "~/models/budget";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

interface ICreateBudgetMutation {
  isCopying: boolean;
}

export const useCreateBudgetMutation = (
  { isCopying }: ICreateBudgetMutation = { isCopying: false },
) => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (newBudget: IBudgetCreateRequest[]) =>
      await request({
        url: "/api/budget",
        method: "POST",
        data: newBudget,
        params: { isCopying },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [budgetsQueryKey] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        message: translateAxiosError(error),
        color: "var(--button-color-destructive)",
      });
    },
  });
};
