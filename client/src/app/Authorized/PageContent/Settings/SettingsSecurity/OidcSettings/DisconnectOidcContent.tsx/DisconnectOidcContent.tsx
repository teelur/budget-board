import { Stack } from "@mantine/core";
import { Button } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import { useDisconnectOidcLoginMutation } from "~/hooks/mutations/applicationUser/useDisconnectOidcLoginMutation";

const DisconnectOidcContent = (): React.ReactNode => {
  const { t } = useTranslation();
  const disconnectOidcLoginMutation = useDisconnectOidcLoginMutation();

  return (
    <Stack>
      <Button
        variant="filled"
        color="error"
        size="xs"
        onClick={() => disconnectOidcLoginMutation.mutate()}
        loading={disconnectOidcLoginMutation.isPending}
      >
        {t("disconnect_oidc_provider")}
      </Button>
    </Stack>
  );
};

export default DisconnectOidcContent;
