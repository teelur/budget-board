import { Stack } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { IApplicationUser } from "~/models/applicationUser";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import DisconnectOidcContent from "./DisconnectOidcContent/DisconnectOidcContent";
import ConnectOidcContent from "./ConnectOidcContent/ConnectOidcContent";

const OidcSettings = (): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();

  const applicationUserQuery = useQuery({
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

  return (
    <Stack gap="0.25rem">
      <PrimaryText size="sm">{t("oidc_settings")}</PrimaryText>
      <DimmedText size="xs">{t("oidc_settings_description")}</DimmedText>
      {applicationUserQuery.data?.hasOidcLogin ? (
        <DisconnectOidcContent />
      ) : (
        <ConnectOidcContent />
      )}
    </Stack>
  );
};

export default OidcSettings;
