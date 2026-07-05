import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { institutionsQueryKey, translateAxiosError } from "~/helpers/requests";
import { IInstitutionCreateRequest } from "~/models/institution";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useCreateInstitutionMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (newInstitution: IInstitutionCreateRequest) =>
      await request({
        url: "/api/institution",
        method: "POST",
        data: newInstitution,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [institutionsQueryKey] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        message: translateAxiosError(error),
        color: "var(--button-color-destructive)",
      });
    },
  });
};
