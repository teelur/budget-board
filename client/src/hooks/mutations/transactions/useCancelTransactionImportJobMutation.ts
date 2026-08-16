import { notifications } from "@mantine/notifications";
import { useMutation } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useCancelTransactionImportJobMutation = () => {
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (jobId: string) =>
      await request({
        url: `/api/transaction/import/${jobId}/cancel`,
        method: "POST",
      }),
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });
};
