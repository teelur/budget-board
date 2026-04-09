import React from "react";
import { Filters } from "~/models/transaction";
import { useDisclosure } from "@mantine/hooks";
import { useNavigate } from "react-router";

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
}

export const TransactionFiltersProvider = (
  props: TransactionFiltersProviderProps,
): React.ReactNode => {
  const navigate = useNavigate();
  const [transactionFilters, setTransactionFilters] = React.useState<Filters>(
    new Filters(),
  );
  const [
    isFiltersPanelOpen,
    { toggle: toggleFiltersPanel, open: openFiltersPanel },
  ] = useDisclosure(false);

  const navigateToTransactions = React.useCallback(
    (filters: Filters): void => {
      setTransactionFilters(filters);
      openFiltersPanel();
      navigate("/transactions");
    },
    [navigate, openFiltersPanel],
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
      isFiltersPanelOpen,
      toggleFiltersPanel,
      navigateToTransactions,
    ],
  );

  return (
    <TransactionFiltersContext.Provider value={value}>
      {props.children}
    </TransactionFiltersContext.Provider>
  );
};

export const useTransactionFilters = () =>
  React.useContext(TransactionFiltersContext);
