import { Stack } from "@mantine/core";
import DarkModeToggle from "./DarkModeToggle/DarkModeToggle";
import LinkSimpleFin from "./LinkSimpleFin/LinkSimpleFin";
import React from "react";
import ResetPassword from "./ResetPassword/ResetPassword";
import UserSettings from "./UserSettings/UserSettings";
import TwoFactorAuth from "./TwoFactorAuth/TwoFactorAuth";
import AdvancedSettings from "./AdvancedSettings/AdvancedSettings";
import OidcSettings from "./OidcSettings/OidcSettings";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { IApplicationUser } from "~/models/applicationUser";
import { AxiosResponse } from "axios";
import CreatePassword from "./CreatePassword/CreatePassword";

const Settings = (): React.ReactNode => {
  const { request } = useAuth();

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
    <Stack w="100%" maw={800} gap="1rem">
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
