import { Button, Stack } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import { useDisconnectOidcLoginMutation } from "~/hooks/mutations/applicationUser/useDisconnectOidcLoginMutation";

const DisconnectOidcContent = (): React.ReactNode => {
  const { t } = useTranslation();
  const disconnectOidcLoginMutation = useDisconnectOidcLoginMutation();

  return (
    <Stack>
      <Button
        color="var(--button-color-destructive)"
        onClick={() => disconnectOidcLoginMutation.mutate()}
        loading={disconnectOidcLoginMutation.isPending}
      >
        {t("disconnect_oidc_provider")}
      </Button>
    </Stack>
  );
};

export default DisconnectOidcContent;
