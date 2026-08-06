import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  applicationUserQueryKey,
  translateAxiosError,
  ValidationError,
} from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useTranslation } from "react-i18next";

export type UpdatePasswordData = {
  oldPassword?: string;
  newPassword: string;
};

export const useUpdatePasswordMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();
  const { t } = useTranslation();

  return useMutation({
    mutationFn: async (updatePasswordData: UpdatePasswordData) =>
      await request({
        url: "/api/manage/info",
        method: "POST",
        data: {
          newPassword: updatePasswordData.newPassword,
          oldPassword: updatePasswordData.oldPassword,
        },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [applicationUserQueryKey],
      });

      notifications.show({
        color: "var(--button-color-confirm)",
        message: t("password_updated_successfully"),
      });
    },
    onError: (error: AxiosError) => {
      if (error?.response?.data) {
        const errorData = error.response.data as ValidationError;
        if (
          error.status === 400 &&
          errorData.title === "One or more validation errors occurred."
        ) {
          notifications.show({
            title: t("one_or_more_validation_errors_occurred"),
            color: "var(--button-color-destructive)",
            message: Object.values(errorData.errors).join("\n"),
          });
        }
      } else {
        notifications.show({
          color: "var(--button-color-destructive)",
          message: translateAxiosError(error),
        });
      }
    },
  });
};
