import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { useTranslation } from "react-i18next";
import {
  accountsQueryKey,
  balancesQueryKey,
  transactionsQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { IAutomaticRuleCreateRequest } from "~/models/automaticRule";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useRunAutomaticRuleMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();
  const { t } = useTranslation();

  return useMutation({
    mutationFn: async (automaticRule: IAutomaticRuleCreateRequest) =>
      await request({
        url: "/api/automaticRule/run",
        method: "POST",
        data: automaticRule,
      }),
    onSuccess: async (data) => {
      await queryClient.invalidateQueries({
        queryKey: [transactionsQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [accountsQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [balancesQueryKey],
      });

      notifications.show({
        title: t("rule_executed"),
        message: data?.data ?? t("rule_run_successfully"),
        color: "var(--button-color-confirm)",
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
