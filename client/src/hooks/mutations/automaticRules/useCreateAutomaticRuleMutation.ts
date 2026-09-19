import { NotificationType, showNotification } from "~/helpers/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  automaticRulesQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { IAutomaticRuleCreateRequest } from "~/models/automaticRule";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useCreateAutomaticRuleMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (newAutomaticRule: IAutomaticRuleCreateRequest) =>
      await request({
        url: "/api/automaticRule",
        method: "POST",
        data: newAutomaticRule,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [automaticRulesQueryKey],
      });
    },
    onError: (error: AxiosError) => {
      showNotification({
        message: translateAxiosError(error),
        type: NotificationType.Error,
      });
    },
  });
};
