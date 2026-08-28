import classes from "./TransactionsHeader.module.css";

import { Button, Collapse, Flex, Group, Stack } from "@mantine/core";
import { FilterIcon } from "lucide-react";
import React from "react";
import SortMenu from "./SortMenu/SortMenu";
import { SortDirection } from "~/components/SortButton";
import { Sorts } from "./SortMenu/SortMenuHelpers";
import FilterCard from "./FilterCard/FilterCard";
import CreateTransactionModal from "./CreateTransactionModal/CreateTransactionModal";
import ImportTransactionsModal from "./ImportTransactionsModal/ImportTransactionsModal";
import { useTransactionFilters } from "~/providers/TransactionFiltersProvider/TransactionFiltersProvider";
import { useTranslation } from "react-i18next";
import ExportTransactionsModal from "./ExportTransactionsModal/ExportTransactionsModal";
import MonthToolcards from "~/components/MonthToolcards/MonthToolcards";
import SelectLastNMonths from "~/components/SelectLastNMonths/SelectLastNMonths";
import { Filters, ITransaction } from "~/models/transaction";
import {
  buildTimeToMonthlyTotalsMap,
  getFilteredTransactions,
  sortTransactions,
} from "~/helpers/transactions";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";

interface TransactionsHeaderProps {
  sort: Sorts;
  setSort: (newSort: Sorts) => void;
  sortDirection: SortDirection;
  setSortDirection: (newSortDirection: SortDirection) => void;
  setCurrentViewTransactions: (transactions: ITransaction[]) => void;
  transactions: ITransaction[];
  isQueryPending: boolean;
  selectedMonths: Date[];
  setSelectedMonths: React.Dispatch<React.SetStateAction<Date[]>>;
}

const TransactionsHeader = (
  props: TransactionsHeaderProps,
): React.ReactNode => {
  const { t } = useTranslation();
  const { transactionFilters, isFiltersPanelOpen, toggleFiltersPanel } =
    useTransactionFilters();
  const { allTransactionCategories } = useTransactionCategories();

  const timeToMonthlyTotalsMap = React.useMemo(
    () => buildTimeToMonthlyTotalsMap(props.selectedMonths, props.transactions),
    [props.selectedMonths, props.transactions],
  );

  React.useEffect(() => {
    if (props.isQueryPending) {
      return;
    }

    const filteredTransactions = getFilteredTransactions(
      props.transactions,
      transactionFilters ?? new Filters(),
      allTransactionCategories,
    );

    const sortedFilteredTransactions = sortTransactions(
      filteredTransactions,
      props.sort,
      props.sortDirection,
    );

    props.setCurrentViewTransactions(sortedFilteredTransactions);
  }, [
    props.transactions,
    props.isQueryPending,
    transactionFilters,
    allTransactionCategories,
    props.sort,
    props.sortDirection,
  ]);

  return (
    <Stack className={classes.root}>
      <Flex className={classes.header}>
        <Group className={classes.buttonGroup}>
          <ImportTransactionsModal />
          <ExportTransactionsModal />
          <Button
            variant={isFiltersPanelOpen ? "outline" : "primary"}
            size="sm"
            rightSection={<FilterIcon size="1rem" />}
            onClick={toggleFiltersPanel}
          >
            {t("filters")}
          </Button>
          <CreateTransactionModal />
        </Group>
      </Flex>
      <Collapse expanded={isFiltersPanelOpen} transitionDuration={100}>
        <FilterCard />
      </Collapse>
      <MonthToolcards
        selectedDates={props.selectedMonths}
        setSelectedDates={props.setSelectedMonths}
        timeToMonthlyTotalsMap={timeToMonthlyTotalsMap}
        isPending={props.isQueryPending}
        allowSelectMultiple
        allowFutureMonths
      />
      <Group w="100%" justify="space-between" align="center" wrap="wrap">
        <SortMenu
          currentSort={props.sort}
          setCurrentSort={props.setSort}
          sortDirection={props.sortDirection}
          setSortDirection={props.setSortDirection}
        />
        <SelectLastNMonths
          monthButtons={[3, 6, 12]}
          setSelectedMonths={props.setSelectedMonths}
        />
      </Group>
    </Stack>
  );
};

export default TransactionsHeader;
