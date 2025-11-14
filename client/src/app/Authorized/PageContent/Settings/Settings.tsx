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
import { useQuery } from "@tanstack/react-query";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { IApplicationUser } from "~/models/applicationUser";
import { AxiosResponse } from "axios";
import CreatePassword from "./CreatePassword";

const Settings = (): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);

  const userQuery = useQuery({
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
    <Stack className={classes.root}>
      <Title order={1}>Settings</Title>
      <DarkModeToggle />
      <UserSettings />
      <LinkSimpleFin />
      <TwoFactorAuth />
      <OidcSettings />
      {userQuery.data?.hasLocalLogin ?? true ? (
        <ResetPassword />
      ) : (
        <CreatePassword />
      )}
      <AdvancedSettings />
    </Stack>
  );
};

export default Settings;
