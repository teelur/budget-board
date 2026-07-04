import { Skeleton, Stack } from "@mantine/core";
import React from "react";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import { useApplicationUserQuery } from "~/hooks/queries/useApplicationUserQuery";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import DisconnectOidcContent from "./DisconnectOidcContent.tsx/DisconnectOidcContent";
import ConnectOidcContent from "./ConnectOidcContent/ConnectOidcContent";
import { getProjectEnvVariables } from "~/shared/projectEnvVariables";

const OidcSettings = (): React.ReactNode => {
  const { t } = useTranslation();
  const applicationUserQuery = useApplicationUserQuery();
  const { envVariables } = getProjectEnvVariables();

  if (applicationUserQuery.isPending) {
    return <Skeleton height={100} radius="md" />;
  }

  if (envVariables.VITE_OIDC_ENABLED?.toLowerCase() !== "true") {
    return null;
  }

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
