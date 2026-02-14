import React from "react";
import { useTranslation } from "react-i18next";
import dayjs from "~/shared/dayjs";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "../AuthProvider/AuthProvider";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";

const localeMap: Record<string, string> = {
  "en-us": "en",
  de: "de",
  fr: "fr",
  "zh-hans": "zh-cn",
};

export interface DateContextValue {
  dayjs: typeof dayjs;
  locale: string;
  dateFormat: string;
}

export const DateContext = React.createContext<DateContextValue>({
  dayjs,
  locale: "en",
  dateFormat: "L",
});

export const DateProvider = ({
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

  // Update dayjs locale whenever i18n language changes
  React.useEffect(() => {
    const dayjsLocale = localeMap[i18n.language] || "en";
    dayjs.locale(dayjsLocale);
  }, [i18n.language]);

  const dateValue: DateContextValue = React.useMemo(() => {
    const dayjsLocale = localeMap[i18n.language] || "en";
    const userDateFormat = userSettingsQuery.data?.dateFormat;

    // If user has custom format, use it; otherwise use locale's default "L"
    const dateFormat =
      userDateFormat && userDateFormat !== "default" ? userDateFormat : "L";

    return {
      dayjs,
      locale: dayjsLocale,
      dateFormat,
    };
  }, [i18n.language, userSettingsQuery.data?.dateFormat]);

  return (
    <DateContext.Provider value={dateValue}>{children}</DateContext.Provider>
  );
};

export const useDate = () => React.useContext(DateContext);
