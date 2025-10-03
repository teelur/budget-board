import classes from "./Settings.module.css";

import { Stack, Title } from "@mantine/core";
import DarkModeToggle from "./DarkModeToggle";
import LinkSimpleFin from "./LinkSimpleFin";
import React from "react";
import ResetPassword from "./ResetPassword";
import UserSettings from "./UserSettings";
import TwoFactorAuth from "./TwoFactorAuth";
import AdvancedSettings from "./AdvancedSettings/AdvancedSettings";

const Settings = (): React.ReactNode => {
  return (
    <Stack className={classes.root}>
      <Title order={1}>Settings</Title>
      <DarkModeToggle />
      <UserSettings />
      <LinkSimpleFin />
      <TwoFactorAuth />
      <ResetPassword />
      <AdvancedSettings />
    </Stack>
  );
};

export default Settings;
