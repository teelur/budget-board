import { NotificationType, showNotification } from "~/helpers/notifications";
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

      showNotification({
        type: NotificationType.Success,
        message: t("password_updated_successfully"),
      });
    },
    onError: (error: AxiosError) => {
      if (error?.response?.data) {
        const errorData = error.response.data as ValidationError;
        if (
          error.response?.status === 400 &&
          errorData.title === "One or more validation errors occurred."
        ) {
          showNotification({
            title: t("one_or_more_validation_errors_occurred"),
            type: NotificationType.Error,
            message: Object.values(errorData.errors).join("\n"),
          });
        } else {
          showNotification({
            type: NotificationType.Error,
            message: translateAxiosError(error),
          });
        }
      } else {
        showNotification({
          type: NotificationType.Error,
          message: translateAxiosError(error),
        });
      }
    },
  });
};
