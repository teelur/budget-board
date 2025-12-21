import { Button, Stack } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { IApplicationUser } from "~/models/applicationUser";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";

const OidcSettings = (): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();

  const userQuery = useQuery({
    queryKey: ["user"],
    queryFn: async (): Promise<IApplicationUser | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/applicationUser",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IApplicationUser;
      }

      return undefined;
    },
  });

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

  if (!userQuery.data?.hasOidcLogin) {
    return null;
  }

  return (
    <Card elevation={1}>
      <Stack gap="1rem">
        <PrimaryText size="lg">{t("oidc_settings")}</PrimaryText>
        <Button
          color="var(--button-color-destructive)"
          onClick={() => doDisconnectOidc.mutate()}
          loading={doDisconnectOidc.isPending}
        >
          {t("disconnect_oidc_provider")}
        </Button>
      </Stack>
    </Card>
  );
};

export default OidcSettings;
