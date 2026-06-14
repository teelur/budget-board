import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  accountsQueryKey,
  accountTypesQueryKey,
  institutionsQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { IAccountTypeUpdateRequest } from "~/models/accountType";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useUpdateAccountTypeMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (req: IAccountTypeUpdateRequest) =>
      await request({
        url: "/api/accountType",
        method: "PUT",
        data: req,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [accountTypesQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [institutionsQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [accountsQueryKey] });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });
};
