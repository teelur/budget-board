import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { accountTypesQueryKey, translateAxiosError } from "~/helpers/requests";
import { IAccountTypeCreateRequest } from "~/models/accountType";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useCreateAccountTypeMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (newAccountType: IAccountTypeCreateRequest) =>
      await request({
        url: "/api/accountType",
        method: "POST",
        data: newAccountType,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [accountTypesQueryKey] });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });
};
