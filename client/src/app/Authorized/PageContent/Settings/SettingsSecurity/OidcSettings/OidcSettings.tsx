import { Button, Skeleton, Stack } from "@mantine/core";
import React from "react";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import { useApplicationUserQuery } from "~/hooks/queries/useApplicationUserQuery";
import { useDisconnectOidcLoginMutation } from "~/hooks/mutations/applicationUser/useDisconnectOidcLoginMutation";

const OidcSettings = (): React.ReactNode => {
  const { t } = useTranslation();
  const applicationUserQuery = useApplicationUserQuery();
  const disconnectOidcLoginMutation = useDisconnectOidcLoginMutation();

  if (applicationUserQuery.isPending) {
    return <Skeleton height={100} radius="md" />;
  }

  if (!applicationUserQuery.data?.hasOidcLogin) {
    return null;
  }

  return (
    <Card elevation={1}>
      <Stack gap="1rem">
        <PrimaryText size="lg">{t("oidc_settings")}</PrimaryText>
        <Button
          color="var(--button-color-destructive)"
          onClick={() => disconnectOidcLoginMutation.mutate()}
          loading={disconnectOidcLoginMutation.isPending}
        >
          {t("disconnect_oidc_provider")}
        </Button>
      </Stack>
    </Card>
  );
};

export default OidcSettings;
