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

export interface LocaleContextValue {
  dayjs: typeof dayjs;
  dayjsLocale: string;
  intlLocale: string;
  dateFormat: string;
  longDateFormat: string;
  decimalSeparator: string;
  thousandsSeparator: string;
}

export const LocaleContext = React.createContext<LocaleContextValue>({
  dayjs,
  dayjsLocale: "en",
  intlLocale: "en-US",
  dateFormat: "L",
  longDateFormat: "LL",
  decimalSeparator: ".",
  thousandsSeparator: ",",
});

export const LocaleProvider = ({
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

  const getLongDateFormat = (dateFormat: string): string => {
    switch (dateFormat) {
      case "MM/DD/YYYY":
        return "MMMM D, YYYY";
      case "DD/MM/YYYY":
        return "D MMMM YYYY";
      case "YYYY/MM/DD":
        return "YYYY MMMM D";
      default:
        return "LL";
    }
  };

  // Update dayjs locale whenever i18n language changes
  React.useEffect(() => {
    const dayjsLocale = localeMap[i18n.language] || "en";
    dayjs.locale(dayjsLocale);
  }, [i18n.language]);

  const localeValue: LocaleContextValue = React.useMemo(() => {
    const dayjsLocale = localeMap[i18n.language] || "en";
    const userDateFormat = userSettingsQuery.data?.dateFormat;

    // If user has custom format, use it; otherwise use locale's default "L"
    const dateFormat =
      userDateFormat && userDateFormat !== "default" ? userDateFormat : "L";

    const formatter = new Intl.NumberFormat(dayjsLocale);
    const parts = formatter.formatToParts(1234.56);

    return {
      dayjs,
      dayjsLocale,
      intlLocale: i18n.language,
      dateFormat,
      longDateFormat: getLongDateFormat(dateFormat),
      decimalSeparator:
        parts.find((part) => part.type === "decimal")?.value || ".",
      thousandsSeparator:
        parts.find((part) => part.type === "group")?.value || ",",
    };
  }, [i18n.language, userSettingsQuery.data?.dateFormat]);

  return (
    <LocaleContext.Provider value={localeValue}>
      {children}
    </LocaleContext.Provider>
  );
};

export const useLocale = () => React.useContext(LocaleContext);
