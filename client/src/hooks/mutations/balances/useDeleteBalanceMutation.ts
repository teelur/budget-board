import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { useTranslation } from "react-i18next";
import {
  accountsQueryKey,
  balancesQueryKey,
  institutionsQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

interface UseDeleteBalanceMutationProps {
  accountID: string;
}

export const useDeleteBalanceMutation = ({
  accountID,
}: UseDeleteBalanceMutationProps) => {
  const queryClient = useQueryClient();
  const { request } = useAuth();
  const { t } = useTranslation();

  return useMutation({
    mutationFn: async (balanceId: string) =>
      await request({
        url: `/api/balance`,
        method: "DELETE",
        params: { balanceId },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [balancesQueryKey, accountID],
      });
      await queryClient.invalidateQueries({ queryKey: [accountsQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [institutionsQueryKey] });

      notifications.show({
        color: "var(--button-color-confirm)",
        message: t("balance_deleted_successfully_message"),
      });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });
};
