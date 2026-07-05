import { Button, Stack } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { OidcAuthFlows } from "~/models/oidc";

const ConnectOidcContent = (): React.ReactNode => {
  const { t } = useTranslation();
  const { startOidcLogin, oidcLoading } = useAuth();

  return (
    <Stack>
      <Button
        color="var(--button-color-primary)"
        onClick={() =>
          startOidcLogin && startOidcLogin(false, OidcAuthFlows.Connect)
        }
        loading={oidcLoading}
      >
        {t("connect_oidc_provider")}
      </Button>
    </Stack>
  );
};

export default ConnectOidcContent;
