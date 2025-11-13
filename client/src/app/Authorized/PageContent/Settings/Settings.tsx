import classes from "./Settings.module.css";

import { Stack, Title } from "@mantine/core";
import DarkModeToggle from "./DarkModeToggle";
import LinkSimpleFin from "./LinkSimpleFin";
import React from "react";
import ResetPassword from "./ResetPassword";
import UserSettings from "./UserSettings";
import TwoFactorAuth from "./TwoFactorAuth";
import AdvancedSettings from "./AdvancedSettings/AdvancedSettings";
import OidcSettings from "./OidcSettings";

const Settings = (): React.ReactNode => {
  return (
    <Stack className={classes.root}>
      <Title order={1}>Settings</Title>
      <DarkModeToggle />
      <UserSettings />
      <LinkSimpleFin />
      <TwoFactorAuth />
      <OidcSettings />
      <ResetPassword />
      <AdvancedSettings />
    </Stack>
  );
};

export default Settings;
