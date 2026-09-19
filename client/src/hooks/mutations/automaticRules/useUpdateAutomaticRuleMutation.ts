import { NotificationType, showNotification } from "~/helpers/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  automaticRulesQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { IAutomaticRuleUpdateRequest } from "~/models/automaticRule";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useUpdateAutomaticRuleMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (data: IAutomaticRuleUpdateRequest) => {
      await request({
        url: `/api/automaticRule`,
        method: "PUT",
        data,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
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
