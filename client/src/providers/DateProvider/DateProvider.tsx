import React from "react";
import { useTranslation } from "react-i18next";
import dayjs from "~/shared/dayjs";

export interface DateContextValue {
  dayjs: typeof dayjs;
  locale: string;
}

export const DateContext = React.createContext<DateContextValue>({
  dayjs,
  locale: "en",
});

export const DateProvider = ({
  children,
}: {
  children: React.ReactNode;
}): React.ReactNode => {
  const { i18n } = useTranslation();

  const localeMap: Record<string, string> = {
    "en-us": "en",
    de: "de",
    fr: "fr",
    "zh-hans": "zh-cn",
  };

  // Update dayjs locale whenever i18n language changes
  React.useEffect(() => {
    const dayjsLocale = localeMap[i18n.language] || "en";
    dayjs.locale(dayjsLocale);
  }, [i18n.language]);

  const dateValue: DateContextValue = React.useMemo(() => {
    const dayjsLocale = localeMap[i18n.language] || "en";

    return {
      dayjs,
      locale: dayjsLocale,
    };
  }, [i18n.language]);

  return (
    <DateContext.Provider value={dateValue}>{children}</DateContext.Provider>
  );
};

export const useDate = () => React.useContext(DateContext);
