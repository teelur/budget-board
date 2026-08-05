import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  accountsQueryKey,
  institutionsQueryKey,
  simpleFinAccountQueryKey,
  simpleFinOrganizationQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useUpdateLinkedAccountMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (updateLinkedAccountRequest: {
      simpleFinAccountGuid: string;
      linkedAccountGuid: string | null;
    }) =>
      await request({
        url: "/api/simpleFinAccount/updateLinkedAccount",
        method: "PUT",
        params: {
          simpleFinAccountGuid: updateLinkedAccountRequest.simpleFinAccountGuid,
          linkedAccountGuid: updateLinkedAccountRequest.linkedAccountGuid,
        },
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: [simpleFinOrganizationQueryKey],
      });
      queryClient.invalidateQueries({ queryKey: [simpleFinAccountQueryKey] });
      queryClient.invalidateQueries({ queryKey: [institutionsQueryKey] });
      queryClient.invalidateQueries({ queryKey: [accountsQueryKey] });
    },
    onError: (error: any) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });
};
