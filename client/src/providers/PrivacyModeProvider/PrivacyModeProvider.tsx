import React from "react";
import { getStoredPrivacyMode, storePrivacyMode } from "~/helpers/privacy";

export interface PrivacyModeContextValue {
  isPrivacyModeEnabled: boolean;
  togglePrivacyMode: () => void;
}

export const PrivacyModeContext =
  React.createContext<PrivacyModeContextValue>({
    isPrivacyModeEnabled: false,
    togglePrivacyMode: () => {},
  });

export const PrivacyModeProvider = ({
  children,
}: {
  children: React.ReactNode;
}): React.ReactNode => {
  const [isPrivacyModeEnabled, setIsPrivacyModeEnabled] =
    React.useState(getStoredPrivacyMode);

  const togglePrivacyMode = React.useCallback(() => {
    setIsPrivacyModeEnabled((enabled) => {
      const nextValue = !enabled;
      storePrivacyMode(nextValue);
      return nextValue;
    });
  }, []);

  const value = React.useMemo(
    () => ({
      isPrivacyModeEnabled,
      togglePrivacyMode,
    }),
    [isPrivacyModeEnabled, togglePrivacyMode],
  );

  return (
    <PrivacyModeContext.Provider value={value}>
      {children}
    </PrivacyModeContext.Provider>
  );
};

export const usePrivacyMode = () => React.useContext(PrivacyModeContext);
