import classes from "./Transactions.module.css";

import { Stack } from "@mantine/core";
import TransactionsHeader from "./TransactionsHeader/TransactionsHeader";
import React from "react";
import { SortDirection } from "~/components/SortButton";
import { Sorts } from "./TransactionsHeader/SortMenu/SortMenuHelpers";
import TransactionCards from "./TransactionCards/TransactionCards";
import BulkActionBar from "../../../../components/BulkActionBar/BulkActionBar";
import { ITransaction } from "~/models/transaction";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

const Transactions = (): React.ReactNode => {
  const { dayjs } = useLocale();
  const { allTransactionCategories: transactionCategories } =
    useTransactionCategories();

  const [sort, setSort] = React.useState(Sorts.Date);
  const [sortDirection, setSortDirection] = React.useState<SortDirection>(
    SortDirection.Decending,
  );
  const [selectedIds, setSelectedIds] = React.useState<Set<string>>(new Set());
  const [selectedMonths, setSelectedMonths] = React.useState<Date[]>([
    dayjs().startOf("month").toDate(),
  ]);
  const [currentViewTransactions, setCurrentViewTransactions] = React.useState<
    ITransaction[]
  >([]);

  const onToggleSelect = (id: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  };

  const onClearSelection = () => setSelectedIds(new Set());

  const onSelectAll = (ids: string[]) => setSelectedIds(new Set(ids));

  return (
    <Stack className={classes.root}>
      <TransactionsHeader
        sort={sort}
        setSort={setSort}
        sortDirection={sortDirection}
        setSortDirection={setSortDirection}
        setCurrentViewTransactions={setCurrentViewTransactions}
        selectedMonths={selectedMonths}
        setSelectedMonths={setSelectedMonths}
      />
      <TransactionCards
        currentViewTransactions={currentViewTransactions}
        isQueryPending={false}
        selectedIds={selectedIds}
        onToggleSelect={onToggleSelect}
      />
      <BulkActionBar
        selectedIds={selectedIds}
        currentPageTransactions={currentViewTransactions}
        onClearSelection={onClearSelection}
        onSelectAll={onSelectAll}
        categories={transactionCategories}
      />
    </Stack>
  );
};

export default Transactions;
