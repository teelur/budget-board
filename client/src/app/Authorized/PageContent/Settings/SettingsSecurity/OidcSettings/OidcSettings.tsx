import { Skeleton, Stack } from "@mantine/core";
import { BodyText } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import { useApplicationUserQuery } from "~/hooks/queries/useApplicationUserQuery";
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
      <Stack gap={0}>
        <BodyText tone="primary" size="sm" fw={500}>
          {t("oidc_settings")}
        </BodyText>
        <BodyText tone="secondary" size="xs">
          {t("oidc_settings_description")}
        </BodyText>
      </Stack>
      {applicationUserQuery.data?.hasOidcLogin ? (
        <DisconnectOidcContent />
      ) : (
        <ConnectOidcContent />
      )}
    </Stack>
  );
};

export default OidcSettings;
