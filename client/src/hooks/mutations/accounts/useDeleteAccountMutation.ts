import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  accountsQueryKey,
  institutionsQueryKey,
  lunchFlowAccountQueryKey,
  simpleFinAccountQueryKey,
  simpleFinOrganizationQueryKey,
  transactionsQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useDeleteAccountMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async ({
      accountId,
      deleteTransactions,
    }: {
      accountId: string;
      deleteTransactions: boolean;
    }) =>
      await request({
        url: "/api/account",
        method: "DELETE",
        params: {
          accountId,
          deleteTransactions,
        },
      }),
    onSuccess: async (_, variables) => {
      await queryClient.invalidateQueries({ queryKey: [accountsQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [institutionsQueryKey] });
      if (variables.deleteTransactions) {
        await queryClient.invalidateQueries({
          queryKey: [transactionsQueryKey],
        });
      }
      await queryClient.invalidateQueries({
        queryKey: [lunchFlowAccountQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [simpleFinOrganizationQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [simpleFinAccountQueryKey],
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        message: translateAxiosError(error),
        color: "var(--button-color-destructive)",
      });
    },
  });
};
