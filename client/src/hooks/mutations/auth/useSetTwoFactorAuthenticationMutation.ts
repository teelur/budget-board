import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import {
  applicationUserQueryKey,
  translateAxiosError,
  twoFactorAuthQueryKey,
  ValidationError,
} from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useTranslation } from "react-i18next";
import {
  TwoFactorAuthRequest,
  TwoFactorAuthResponse,
} from "~/models/twoFactorAuth";

type SetTwoFactorAuthenticationVariables = {
  twoFactorAuthData: TwoFactorAuthRequest;
  setRecoveryCodes: (codes: string[]) => void;
};

export const useSetTwoFactorAuthenticationMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();
  const { t } = useTranslation();

  return useMutation({
    mutationFn: async ({
      twoFactorAuthData,
    }: SetTwoFactorAuthenticationVariables) =>
      await request({
        url: "/api/manage/2fa",
        method: "POST",
        data: { ...twoFactorAuthData },
      }),
    onSuccess: async (
      res: AxiosResponse,
      variables: SetTwoFactorAuthenticationVariables,
    ) => {
      await queryClient.invalidateQueries({
        queryKey: [applicationUserQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [twoFactorAuthQueryKey],
      });

      const data = res.data as TwoFactorAuthResponse;
      if (!data) {
        notifications.show({
          color: "var(--button-color-destructive)",
          message: t("no_data_returned_from_server"),
        });
        return;
      }

      notifications.show({
        color: "var(--button-color-confirm)",
        message: t("two_factor_auth_successfully_updated"),
      });

      variables.setRecoveryCodes(data.recoveryCodes || []);
    },
    onError: (error: AxiosError) => {
      if (error?.response?.data) {
        const errorData = error.response.data as ValidationError;
        if (
          error.response?.status === 400 &&
          errorData.title === "One or more validation errors occurred."
        ) {
          notifications.show({
            title: t("one_or_more_validation_errors_occurred"),
            color: "var(--button-color-destructive)",
            message: Object.values(errorData.errors).join("\n"),
          });
        } else {
          notifications.show({
            color: "var(--button-color-destructive)",
            message: translateAxiosError(error),
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
