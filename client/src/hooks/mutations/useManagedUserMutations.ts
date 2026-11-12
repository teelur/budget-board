import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  applicationUserQueryKey,
  managedUsersQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import type {
  ICreateManagedUserRequest,
  IManagedUserOperationResponse,
  IUpdateManagedUserUsernameRequest,
} from "~/models/managedUser";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { NotificationType, showNotification } from "~/helpers/notifications";
import { useTranslation } from "react-i18next";

const notifyError = (error: AxiosError) =>
  showNotification({
    type: NotificationType.Error,
    message: translateAxiosError(error),
  });

export const useCreateManagedUserMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (
      user: ICreateManagedUserRequest,
    ): Promise<IManagedUserOperationResponse> => {
      const response = await request({
        url: "/api/admin/users",
        method: "POST",
        data: user,
      });

      return response.data as IManagedUserOperationResponse;
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [managedUsersQueryKey] });
    },
    onError: notifyError,
  });
};

export const useUpdateManagedUserUsernameMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();
  const { t } = useTranslation();

  return useMutation({
    mutationFn: async ({
      userId,
      ...usernameRequest
    }: { userId: string } & IUpdateManagedUserUsernameRequest) => {
      await request({
        url: `/api/admin/users/${userId}/username`,
        method: "PUT",
        data: usernameRequest,
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [managedUsersQueryKey] });
      showNotification({
        type: NotificationType.Success,
        message: t("managed_user_username_updated_successfully"),
      });
    },
    onError: (error: AxiosError) => {
      const responseData: unknown = error.response?.data;
      if (
        typeof responseData === "object" &&
        responseData !== null &&
        "detail" in responseData &&
        responseData.detail === "UsernameAlreadyTaken"
      ) {
        showNotification({
          type: NotificationType.Error,
          message: t("username_already_taken"),
        });
        return;
      }

      notifyError(error);
    },
  });
};

export const useResetManagedUserPasswordMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();

  return useMutation({
    mutationFn: async (
      userId: string,
    ): Promise<IManagedUserOperationResponse> => {
      const response = await request({
        url: `/api/admin/users/${userId}/reset-password`,
        method: "POST",
      });

      return response.data as IManagedUserOperationResponse;
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [managedUsersQueryKey] });
    },
    onError: notifyError,
  });
};

export const useSetManagedUserDisabledMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();
  const { t } = useTranslation();

  return useMutation({
    mutationFn: async ({
      userId,
      disabled,
    }: {
      userId: string;
      disabled: boolean;
    }) => {
      const action = disabled ? "disable" : "enable";
      await request({
        url: `/api/admin/users/${userId}/${action}`,
        method: "POST",
      });
    },
    onSuccess: async (_response, variables) => {
      await queryClient.invalidateQueries({ queryKey: [managedUsersQueryKey] });
      showNotification({
        type: NotificationType.Success,
        message: variables.disabled
          ? t("managed_user_disabled_successfully")
          : t("managed_user_enabled_successfully"),
      });
    },
    onError: notifyError,
  });
};

export const useTransferSuperuserMutation = () => {
  const queryClient = useQueryClient();
  const { request } = useAuth();
  const { t } = useTranslation();

  return useMutation({
    mutationFn: async (userId: string) => {
      await request({
        url: `/api/admin/users/${userId}/transfer-superuser`,
        method: "POST",
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [applicationUserQueryKey],
      });
      showNotification({
        type: NotificationType.Success,
        message: t("superuser_transferred_successfully"),
      });
    },
    onError: notifyError,
  });
};
