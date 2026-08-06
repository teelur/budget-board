import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { translateAxiosError, userSettingsQueryKey } from "~/helpers/requests";
import { ITrainAutoCategorizerRequest } from "~/models/autoCategorizer";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useTrainAutomaticTransactionCategorizerMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (trainAutoCategorizer: ITrainAutoCategorizerRequest) =>
      await request({
        url: "/api/automaticTransactionCategorizer/train",
        method: "POST",
        data: trainAutoCategorizer,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [userSettingsQueryKey] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        message: translateAxiosError(error),
        color: "var(--button-color-destructive)",
      });
    },
  });
};
