import {
  getTransactionsByCategory,
  getVisibleTransactions,
} from "~/helpers/transactions";
import { Group, Pagination, ScrollArea, Skeleton, Stack } from "@mantine/core";
import { BodyText, Card, HeadingText } from "@teelur/budget-board-ui";
import { ITransaction } from "~/models/transaction";
import React from "react";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { useTranslation } from "react-i18next";
import { TagsIcon } from "lucide-react";
import BulkActionBar from "~/components/BulkActionBar/BulkActionBar";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";
import TransactionCard from "~/components/core/TransactionCard/TransactionCard";

const UncategorizedTransactionsWidget = (): React.ReactNode => {
  const itemsPerPage = 15;
  const [activePage, setPage] = React.useState(1);
  const [selectedIds, setSelectedIds] = React.useState<Set<string>>(new Set());

  const { t } = useTranslation();
  const { allTransactionCategories: transactionCategories } =
    useTransactionCategories();
  const transactionsQuery = useTransactionsQuery();

  const sortedFilteredTransactions = React.useMemo(
    () =>
      getVisibleTransactions(
        getTransactionsByCategory(transactionsQuery.data ?? [], ""),
      ).sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime()),
    [transactionsQuery.data],
  );

  const currentPageTransactions = React.useMemo(
    () =>
      sortedFilteredTransactions.slice(
        (activePage - 1) * itemsPerPage,
        (activePage - 1) * itemsPerPage + itemsPerPage,
      ),
    [sortedFilteredTransactions, activePage, itemsPerPage],
  );

  const onToggleSelect = (id: string) =>
    setSelectedIds((prev) => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });

  const onClearSelection = () => setSelectedIds(new Set());

  const onSelectAll = (ids: string[]) =>
    setSelectedIds((prev) => {
      const next = new Set(prev);
      ids.forEach((id) => next.add(id));
      return next;
    });

  const getWidgetContent = () => {
    if (transactionsQuery.isPending) {
      return <Skeleton h="100%" radius="md" />;
    }

    if (sortedFilteredTransactions.length === 0) {
      return (
        <BodyText tone="muted" size="sm" ta="center">
          {t("no_uncategorized_transactions")}
        </BodyText>
      );
    }

    return (
      <ScrollArea
        w="100%"
        type="auto"
        offsetScrollbars="present"
        style={{ flex: "1 1 0", minHeight: 0 }}
      >
        <Stack gap="0.3rem">
          {currentPageTransactions.map((transaction: ITransaction) => (
            <TransactionCard
              key={transaction.id}
              transaction={transaction}
              categories={transactionCategories}
              selected={selectedIds.has(transaction.id)}
              onToggleSelect={onToggleSelect}
              surface="sunken"
            />
          ))}
        </Stack>
      </ScrollArea>
    );
  };

  return (
    <>
      <Card
        w="100%"
        h="100%"
        style={{ display: "flex", flexDirection: "column", minHeight: 0 }}
      >
        <Card.Section separatorThickness="thick">
          <Group gap="0.25rem">
            <TagsIcon size="1.5rem" color="var(--bbui-color-primary)" />
            <HeadingText tone="heading" level={4}>
              {t("uncategorized_transactions")}
            </HeadingText>
          </Group>
        </Card.Section>
        <Card.Section
          p="0.5rem"
          style={{
            display: "flex",
            flex: "1 1 0",
            flexDirection: "column",
            minHeight: 0,
          }}
        >
          <Stack
            gap="0.5rem"
            justify="center"
            align="center"
            w="100%"
            style={{ flex: "1 1 0", minHeight: 0 }}
          >
            {getWidgetContent()}
            {sortedFilteredTransactions.length > itemsPerPage && (
              <Pagination
                value={activePage}
                onChange={setPage}
                total={Math.ceil(
                  sortedFilteredTransactions.length / itemsPerPage,
                )}
              />
            )}
          </Stack>
        </Card.Section>
      </Card>
      <BulkActionBar
        selectedIds={selectedIds}
        currentPageTransactions={currentPageTransactions}
        onClearSelection={onClearSelection}
        onSelectAll={onSelectAll}
        categories={transactionCategories}
      />
    </>
  );
};

export default UncategorizedTransactionsWidget;
