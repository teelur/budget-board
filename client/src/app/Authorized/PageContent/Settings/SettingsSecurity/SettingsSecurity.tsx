import { Skeleton, Stack } from "@mantine/core";
import React from "react";
import TwoFactorAuth from "./TwoFactorAuth/TwoFactorAuth";
import OidcSettings from "./OidcSettings/OidcSettings";
import ResetPassword from "./ResetPassword/ResetPassword";
import CreatePassword from "./CreatePassword/CreatePassword";
import { useApplicationUserQuery } from "~/hooks/queries/useApplicationUserQuery";

const SettingsSecurity = (): React.ReactNode => {
  const applicationUserQuery = useApplicationUserQuery();

  const getPasswordManagementComponent = (): React.ReactNode => {
    if (applicationUserQuery.isPending) {
      return <Skeleton height={250} radius="md" />;
    }

    if (!applicationUserQuery.data) {
      return null;
    }

    if (applicationUserQuery.data.hasLocalLogin) {
      return <ResetPassword />;
    } else {
      return <CreatePassword />;
    }
  };

  return (
    <Stack gap="1rem">
      <OidcSettings />
      <TwoFactorAuth />
      {getPasswordManagementComponent()}
    </Stack>
  );
};

export default SettingsSecurity;
