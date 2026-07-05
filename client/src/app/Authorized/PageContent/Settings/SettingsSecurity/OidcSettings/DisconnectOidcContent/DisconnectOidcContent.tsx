import { Button, Stack } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { useTranslation } from "react-i18next";
import { translateAxiosError } from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
const DisconnectOidcContent = (): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doDisconnectOidc = useMutation({
    mutationFn: async () =>
      await request({
        url: "/api/applicationUser/disconnectOidcLogin",
        method: "DELETE",
      }),
    onSuccess: async (res: AxiosResponse) => {
      await queryClient.invalidateQueries({ queryKey: ["user"] });

      notifications.show({
        color: "var(--button-color-confirm)",
        message:
          res?.data?.message ?? t("oidc_provider_disconnected_successfully"),
      });
    },
    onError: (error: any) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  return (
    <Stack>
      {" "}
      <Button
        color="var(--button-color-destructive)"
        onClick={() => doDisconnectOidc.mutate()}
        loading={doDisconnectOidc.isPending}
      >
        {" "}
        {t("disconnect_oidc_provider")}{" "}
      </Button>{" "}
    </Stack>
  );
};
export default DisconnectOidcContent;
