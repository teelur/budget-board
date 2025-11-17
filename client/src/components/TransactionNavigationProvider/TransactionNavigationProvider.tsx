import React from "react";
import { Filters } from "~/models/transaction";
import { Pages } from "~/app/Authorized/PageContent/PageContent";
import { useDisclosure } from "@mantine/hooks";

interface TransactionFiltersContextType {
  transactionFilters: Filters;
  setTransactionFilters: (filters: Filters) => void;
  isFiltersPanelOpen: boolean;
  toggleFiltersPanel: () => void;
  navigateToTransactions: (filters: Filters) => void;
}

export const TransactionFiltersContext =
  React.createContext<TransactionFiltersContextType>({
    transactionFilters: new Filters(),
    setTransactionFilters: () => {},
    isFiltersPanelOpen: false,
    toggleFiltersPanel: () => {},
    navigateToTransactions: () => {},
  });

interface TransactionFiltersProviderProps {
  children: React.ReactNode;
  setCurrentPage: (page: Pages) => void;
}

export const TransactionFiltersProvider = (
  props: TransactionFiltersProviderProps
): React.ReactNode => {
  const [transactionFilters, setTransactionFilters] = React.useState<Filters>(
    new Filters()
  );
  const [
    isFiltersPanelOpen,
    { toggle: toggleFiltersPanel, open: openFiltersPanel },
  ] = useDisclosure(false);

  const navigateToTransactions = React.useCallback(
    (filters: Filters): void => {
      setTransactionFilters(filters);
      openFiltersPanel();
      props.setCurrentPage(Pages.Transactions);
    },
    [props]
  );

  const value = React.useMemo(
    () => ({
      transactionFilters,
      setTransactionFilters,
      isFiltersPanelOpen,
      toggleFiltersPanel,
      navigateToTransactions,
    }),
    [
      transactionFilters,
      setTransactionFilters,
      isFiltersPanelOpen,
      toggleFiltersPanel,
      navigateToTransactions,
    ]
  );

  return (
    <TransactionFiltersContext.Provider value={value}>
      {props.children}
    </TransactionFiltersContext.Provider>
  );
};

export const useTransactionFilters = () => {
  const context = React.useContext(TransactionFiltersContext);
  if (!context) {
    throw new Error(
      "useTransactionFilters must be used within TransactionFiltersProvider"
    );
  }
  return context;
};
