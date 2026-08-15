import classes from "./TransactionsHeader.module.css";

import {
  ActionIcon,
  Button,
  Collapse,
  Flex,
  Group,
  Stack,
} from "@mantine/core";
import { FilterIcon, SettingsIcon } from "lucide-react";
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
import { useNavigate } from "react-router";
import MonthToolcards from "~/components/MonthToolcards/MonthToolcards";
import { Filters, ITransaction } from "~/models/transaction";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
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
  setCurrentViewTransactions: React.Dispatch<
    React.SetStateAction<ITransaction[]>
  >;
  selectedMonths: Date[];
  setSelectedMonths: React.Dispatch<React.SetStateAction<Date[]>>;
}

const TransactionsHeader = (
  props: TransactionsHeaderProps,
): React.ReactNode => {
  const navigate = useNavigate();

  const { t } = useTranslation();
  const { transactionFilters, isFiltersPanelOpen, toggleFiltersPanel } =
    useTransactionFilters();
  const { dayjs } = useLocale();
  const { allTransactionCategories } = useTransactionCategories();
  const transactionsQuery = useTransactionsQuery({
    selectedDates: props.selectedMonths.map((date) => ({
      month: dayjs(date).month() + 1,
      year: dayjs(date).year(),
    })),
    includeHiddenCategory: true,
  });
  const timeToMonthlyTotalsMap = React.useMemo(
    () =>
      buildTimeToMonthlyTotalsMap(
        props.selectedMonths,
        transactionsQuery.data ?? [],
      ),
    [props.selectedMonths, transactionsQuery.data],
  );

  React.useEffect(() => {
    const filteredTransactions = getFilteredTransactions(
      transactionsQuery.data ?? [],
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
    transactionsQuery.data,
    transactionFilters,
    allTransactionCategories,
    props.sort,
    props.sortDirection,
  ]);

  return (
    <Stack className={classes.root}>
      <Flex className={classes.header}>
        <SortMenu
          currentSort={props.sort}
          setCurrentSort={props.setSort}
          sortDirection={props.sortDirection}
          setSortDirection={props.setSortDirection}
        />
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
          <ActionIcon
            variant="subtle"
            size="input-sm"
            onClick={() => navigate("/transactions/settings")}
          >
            <SettingsIcon />
          </ActionIcon>
        </Group>
      </Flex>
      <Collapse expanded={isFiltersPanelOpen} transitionDuration={100}>
        <FilterCard />
      </Collapse>
      <MonthToolcards
        selectedDates={props.selectedMonths}
        setSelectedDates={props.setSelectedMonths}
        timeToMonthlyTotalsMap={timeToMonthlyTotalsMap}
        isPending={transactionsQuery.isPending}
        allowSelectMultiple
      />
    </Stack>
  );
};

export default TransactionsHeader;
