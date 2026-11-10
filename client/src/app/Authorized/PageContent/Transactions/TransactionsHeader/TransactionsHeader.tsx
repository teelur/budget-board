import classes from "./TransactionsHeader.module.css";

import { Collapse, Flex, Group, Menu, Stack } from "@mantine/core";
import { Button, Modal, MonthToolcards } from "@teelur/budget-board-ui";
import { useDisclosure } from "@mantine/hooks";
import {
  CalendarDaysIcon,
  FileDownIcon,
  FileUpIcon,
  FilterIcon,
  ListChecksIcon,
  MoreHorizontalIcon,
} from "lucide-react";
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
import SelectLastNMonths from "~/components/SelectLastNMonths/SelectLastNMonths";
import { Filters, ITransaction } from "~/models/transaction";
import {
  buildTimeToMonthlyTotalsMap,
  getFilteredTransactions,
  sortTransactions,
} from "~/helpers/transactions";
import { getMonthToolcardStatus } from "~/helpers/monthToolcards";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { getMonthsInDateRange } from "~/helpers/datetime";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import useIsMobile from "~/hooks/useIsMobile";

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
  const { dayjs, dayjsLocale } = useLocale();
  const isMobile = useIsMobile();
  const [canSelectMultiple, { toggle, open: openMultipleSelection }] =
    useDisclosure(false);
  const [isMonthPickerOpen, { open: openMonthPicker, close: closeMonthPicker }] =
    useDisclosure(false);
  const [isImportModalOpen, { open: openImportModal, close: closeImportModal }] =
    useDisclosure(false);
  const [isExportModalOpen, { open: openExportModal, close: closeExportModal }] =
    useDisclosure(false);

  const toggleSelectMultiple = () => {
    if (canSelectMultiple) {
      if (props.selectedMonths.length === 0) {
        props.setSelectedMonths([dayjs().startOf("month").toDate()]);
      } else {
        props.setSelectedMonths([
          new Date(
            Math.max(...props.selectedMonths.map((date) => date.getTime())),
          ),
        ]);
      }
    }

    toggle();
  };

  const timeToMonthlyTotalsMap = React.useMemo(
    () => buildTimeToMonthlyTotalsMap(props.selectedMonths, props.transactions),
    [props.selectedMonths, props.transactions],
  );

  const getMonthStatus = React.useCallback(
    (date: Date) => getMonthToolcardStatus(timeToMonthlyTotalsMap, date),
    [timeToMonthlyTotalsMap],
  );

  React.useEffect(() => {
    const [startDate, endDate] = transactionFilters.dateRange;

    if (startDate && endDate) {
      const monthsInDateRange = getMonthsInDateRange(startDate, endDate);

      props.setSelectedMonths((selectedMonths) => {
        const isSelectionUnchanged =
          selectedMonths.length === monthsInDateRange.length &&
          selectedMonths.every(
            (month, index) =>
              month.getTime() === monthsInDateRange[index]?.getTime(),
          );

        return isSelectionUnchanged ? selectedMonths : monthsInDateRange;
      });

      if (monthsInDateRange.length > 1) {
        openMultipleSelection();
      }
    }
  }, [
    transactionFilters.dateRange,
    props.setSelectedMonths,
    openMultipleSelection,
  ]);

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

  const monthButtonLabel =
    props.selectedMonths.length === 1
      ? dayjs(props.selectedMonths[0])
          .locale(dayjsLocale)
          .format("MMM YYYY")
      : props.selectedMonths.length > 1
        ? `${t("select_months")} (${props.selectedMonths.length})`
        : t("select_months");

  return (
    <Stack className={classes.root}>
      {isMobile ? (
        <>
          <Group className={classes.mobileToolbar} gap="0.375rem" wrap="nowrap">
            <Button
              className={classes.monthButton}
              variant="filled"
              color="primary"
              size="compact-sm"
              leftSection={<CalendarDaysIcon size="1rem" />}
              aria-label={`${t("select_months")}: ${monthButtonLabel}`}
              onClick={openMonthPicker}
            >
              <span className={classes.monthButtonLabel}>
                {monthButtonLabel}
              </span>
            </Button>
            <Button
              variant="filled"
              color="primary"
              size="compact-sm"
              selected={
                isFiltersPanelOpen || !transactionFilters.isEqual(new Filters())
              }
              aria-label={t("filters")}
              onClick={toggleFiltersPanel}
            >
              <FilterIcon size="1rem" />
            </Button>
            <SortMenu
              compact
              currentSort={props.sort}
              setCurrentSort={props.setSort}
              sortDirection={props.sortDirection}
              setSortDirection={props.setSortDirection}
            />
            <CreateTransactionModal />
            <Menu>
              <Menu.Target>
                <Button
                  variant="filled"
                  color="primary"
                  size="compact-sm"
                  aria-label={t("more_options")}
                >
                  <MoreHorizontalIcon size="1rem" />
                </Button>
              </Menu.Target>
              <Menu.Dropdown>
                <Menu.Item
                  leftSection={<FileDownIcon size="1rem" />}
                  onClick={openImportModal}
                >
                  {t("import")}
                </Menu.Item>
                <Menu.Item
                  leftSection={<FileUpIcon size="1rem" />}
                  onClick={openExportModal}
                >
                  {t("export")}
                </Menu.Item>
              </Menu.Dropdown>
            </Menu>
          </Group>
          <ImportTransactionsModal
            opened={isImportModalOpen}
            onOpen={openImportModal}
            onClose={closeImportModal}
            showTrigger={false}
          />
          <ExportTransactionsModal
            opened={isExportModalOpen}
            onOpen={openExportModal}
            onClose={closeExportModal}
            showTrigger={false}
          />
        </>
      ) : (
        <Flex className={classes.header}>
          <Group className={classes.buttonGroup}>
            <ImportTransactionsModal />
            <ExportTransactionsModal />
            <Button
              variant="filled"
              color="primary"
              size="xs"
              selected={
                isFiltersPanelOpen ||
                !transactionFilters.isEqual(new Filters())
              }
              rightSection={<FilterIcon size="1rem" />}
              onClick={toggleFiltersPanel}
            >
              {t("filters")}
            </Button>
            <CreateTransactionModal />
          </Group>
        </Flex>
      )}
      <Collapse expanded={isFiltersPanelOpen} transitionDuration={100}>
        <FilterCard />
      </Collapse>
      {isMobile ? (
        <Modal
          opened={isMonthPickerOpen}
          onClose={closeMonthPicker}
          fullScreen
          title={t("select_months")}
        >
          <Stack gap="0.75rem">
            <MonthToolcards
              selectedDates={props.selectedMonths}
              setSelectedDates={props.setSelectedMonths}
              getMonthStatus={getMonthStatus}
              isPending={props.isQueryPending}
              allowSelectMultiple={canSelectMultiple}
              allowFutureMonths
              locale={dayjsLocale}
            />
            <Group justify="space-between" align="center" wrap="wrap">
              <Button
                variant="filled"
                color="primary"
                size="compact-sm"
                selected={canSelectMultiple}
                rightSection={<ListChecksIcon size="1rem" />}
                onClick={toggleSelectMultiple}
              >
                {t("select_multiple")}
              </Button>
              <SelectLastNMonths
                monthButtons={[3, 6, 12]}
                selectedMonths={props.selectedMonths}
                setSelectedMonths={props.setSelectedMonths}
                onSelectMonths={openMultipleSelection}
                size="compact-sm"
              />
            </Group>
          </Stack>
        </Modal>
      ) : (
        <>
          <MonthToolcards
            selectedDates={props.selectedMonths}
            setSelectedDates={props.setSelectedMonths}
            getMonthStatus={getMonthStatus}
            isPending={props.isQueryPending}
            allowSelectMultiple={canSelectMultiple}
            allowFutureMonths
            locale={dayjsLocale}
          />
          <Group w="100%" justify="space-between" align="center" wrap="wrap">
            <SortMenu
              currentSort={props.sort}
              setCurrentSort={props.setSort}
              sortDirection={props.sortDirection}
              setSortDirection={props.setSortDirection}
            />
            <Group gap="0.5rem">
              <Button
                variant="filled"
                color="primary"
                size="compact-sm"
                selected={canSelectMultiple}
                rightSection={<ListChecksIcon size="1rem" />}
                onClick={toggleSelectMultiple}
              >
                {t("select_multiple")}
              </Button>
              <SelectLastNMonths
                monthButtons={[3, 6, 12]}
                selectedMonths={props.selectedMonths}
                setSelectedMonths={props.setSelectedMonths}
                onSelectMonths={openMultipleSelection}
                size="compact-sm"
              />
            </Group>
          </Group>
        </>
      )}
    </Stack>
  );
};

export default TransactionsHeader;
