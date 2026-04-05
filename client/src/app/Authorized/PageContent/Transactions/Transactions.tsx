import classes from "./Transactions.module.css";

import { Stack } from "@mantine/core";
import TransactionsHeader from "./TransactionsHeader/TransactionsHeader";
import React from "react";
import { SortDirection } from "~/components/SortButton";
import { Sorts } from "./TransactionsHeader/SortMenu/SortMenuHelpers";
import TransactionCards from "./TransactionCards.tsx/TransactionCards";
import BulkActionBar from "./BulkActionBar/BulkActionBar";
import { ITransaction } from "~/models/transaction";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";

const Transactions = (): React.ReactNode => {
  const [sort, setSort] = React.useState(Sorts.Date);
  const [sortDirection, setSortDirection] = React.useState<SortDirection>(
    SortDirection.Decending,
  );
  const [selectedIds, setSelectedIds] = React.useState<Set<string>>(new Set());
  const [currentPageTransactions, setCurrentPageTransactions] = React.useState<
    ITransaction[]
  >([]);

  const { transactionCategories } = useTransactionCategories();

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
    <Stack className={classes.root} pb="var(--bulk-bar-height, 0)">
      <TransactionsHeader
        sort={sort}
        setSort={setSort}
        sortDirection={sortDirection}
        setSortDirection={setSortDirection}
      />
      <TransactionCards
        sort={sort}
        sortDirection={sortDirection}
        selectedIds={selectedIds}
        onToggleSelect={onToggleSelect}
        onCurrentPageChange={setCurrentPageTransactions}
      />
      <BulkActionBar
        selectedIds={selectedIds}
        currentPageTransactions={currentPageTransactions}
        onClearSelection={onClearSelection}
        onSelectAll={onSelectAll}
        categories={transactionCategories}
      />
    </Stack>
  );
};

export default Transactions;
