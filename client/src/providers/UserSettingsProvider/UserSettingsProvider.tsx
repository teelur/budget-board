import React from "react";
import { useTranslation } from "react-i18next";
import { useUserSettingsQuery } from "~/hooks/queries/useUserSettingsQuery";

export interface UserSettingsContextValue {
  preferredCurrency: string;
  preferredLanguage: string;
  preferredDateFormat: string;
  budgetWarningThreshold: number;
  forceSyncLookbackMonths: number;
  disableBuiltInAccountTypes: boolean;
  disableBuiltInAssetTypes: boolean;
  disableBuiltInTransactionCategories: boolean;
  enableAutoCategorizer: boolean;
  autoCategorizerModelOID: number | null;
  autoCategorizerLastTrained: Date | null;
  autoCategorizerModelStartDate: Date | null;
  autoCategorizerModelEndDate: Date | null;
  autoCategorizerMinimumProbabilityPercentage: number;
}

export const UserSettingsContext =
  React.createContext<UserSettingsContextValue>({
    preferredCurrency: "USD",
    preferredLanguage: "default",
    preferredDateFormat: "default",
    budgetWarningThreshold: 80,
    forceSyncLookbackMonths: 0,
    disableBuiltInAccountTypes: false,
    disableBuiltInAssetTypes: false,
    disableBuiltInTransactionCategories: false,
    enableAutoCategorizer: false,
    autoCategorizerModelOID: null,
    autoCategorizerLastTrained: null,
    autoCategorizerModelStartDate: null,
    autoCategorizerModelEndDate: null,
    autoCategorizerMinimumProbabilityPercentage: 70,
  });

export const UserSettingsProvider = ({
  children,
}: {
  children: React.ReactNode;
}): React.ReactNode => {
  const { i18n } = useTranslation();
  const userSettingsQuery = useUserSettingsQuery();

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
  }, [userSettingsQuery.data?.language, i18n]);

  const userSettingsValue: UserSettingsContextValue = {
    preferredCurrency: userSettingsQuery.data?.currency ?? "USD",
    preferredLanguage: userSettingsQuery.data?.language ?? "default",
    preferredDateFormat: userSettingsQuery.data?.dateFormat ?? "default",
    budgetWarningThreshold:
      userSettingsQuery.data?.budgetWarningThreshold ?? 80,
    disableBuiltInAccountTypes:
      userSettingsQuery.data?.disableBuiltInAccountTypes ?? false,
    disableBuiltInAssetTypes:
      userSettingsQuery.data?.disableBuiltInAssetTypes ?? false,
    forceSyncLookbackMonths:
      userSettingsQuery.data?.forceSyncLookbackMonths ?? 0,
    disableBuiltInTransactionCategories:
      userSettingsQuery.data?.disableBuiltInTransactionCategories ?? false,
    enableAutoCategorizer:
      userSettingsQuery.data?.enableAutoCategorizer ?? false,
    autoCategorizerModelOID:
      userSettingsQuery.data?.autoCategorizerModelOID ?? null,
    autoCategorizerLastTrained:
      userSettingsQuery.data?.autoCategorizerLastTrained ?? null,
    autoCategorizerModelStartDate:
      userSettingsQuery.data?.autoCategorizerModelStartDate ?? null,
    autoCategorizerModelEndDate:
      userSettingsQuery.data?.autoCategorizerModelEndDate ?? null,
    autoCategorizerMinimumProbabilityPercentage:
      userSettingsQuery.data?.autoCategorizerMinimumProbabilityPercentage ?? 70,
  };

  return (
    <UserSettingsContext.Provider value={userSettingsValue}>
      {children}
    </UserSettingsContext.Provider>
  );
};

export const useUserSettings = () => React.useContext(UserSettingsContext);
