import React from "react";
import { useAuth } from "../AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";
import { useTranslation } from "react-i18next";

export interface UserSettingsContextValue {
  preferredCurrency: string;
  preferredLanguage: string;
}

export const UserSettingsContext =
  React.createContext<UserSettingsContextValue>({
    preferredCurrency: "USD",
    preferredLanguage: "default",
  });

export const UserSettingsProvider = ({
  children,
}: {
  children: React.ReactNode;
}): React.ReactNode => {
  const { i18n } = useTranslation();
  const { request } = useAuth();

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

  React.useEffect(() => {
    if (userSettingsQuery.data?.language) {
      if (userSettingsQuery.data.language === "default") {
        // Let i18n detect and use browser's locale
        const userLanguage = navigator.language;
        i18n.changeLanguage(userLanguage);
      } else {
        // Use explicitly set language preference
        i18n.changeLanguage(userSettingsQuery.data.language);
      }
    }
  }, [userSettingsQuery.data?.language]);

  const userSettingsValue: UserSettingsContextValue = {
    preferredCurrency: userSettingsQuery.data?.currency ?? "USD",
    preferredLanguage: userSettingsQuery.data?.language ?? "default",
  };

  return (
    <UserSettingsContext.Provider value={userSettingsValue}>
      {children}
    </UserSettingsContext.Provider>
  );
};

export const useUserSettings = () => React.useContext(UserSettingsContext);
