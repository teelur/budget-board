import { Stack } from "@mantine/core";
import React from "react";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { IApplicationUser } from "~/models/applicationUser";
import TwoFactorAuth from "./TwoFactorAuth/TwoFactorAuth";
import OidcSettings from "./OidcSettings/OidcSettings";
import ResetPassword from "./ResetPassword/ResetPassword";
import CreatePassword from "./CreatePassword/CreatePassword";

const SettingsSecurity = (): React.ReactNode => {
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
    <Stack gap="1rem">
      <TwoFactorAuth />
      <OidcSettings />
      {(userQuery.data?.hasLocalLogin ?? true) ? (
        <ResetPassword />
      ) : (
        <CreatePassword />
      )}
    </Stack>
  );
};

export default SettingsSecurity;
