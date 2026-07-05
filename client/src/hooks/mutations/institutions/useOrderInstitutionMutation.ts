import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { institutionsQueryKey, translateAxiosError } from "~/helpers/requests";
import { InstitutionIndexRequest } from "~/models/institution";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useOrderInstitutionsMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (orderedInstitutions: InstitutionIndexRequest[]) =>
      await request({
        url: "/api/institution/order",
        method: "PUT",
        data: orderedInstitutions,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [institutionsQueryKey] });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });
};
