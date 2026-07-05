import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  accountsQueryKey,
  institutionsQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { IInstitutionUpdateRequest } from "~/models/institution";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useUpdateInstitutionMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (updatedInstitution: IInstitutionUpdateRequest) => {
      return await request({
        url: "/api/institution",
        method: "PUT",
        data: updatedInstitution,
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [institutionsQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [accountsQueryKey] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });
};
