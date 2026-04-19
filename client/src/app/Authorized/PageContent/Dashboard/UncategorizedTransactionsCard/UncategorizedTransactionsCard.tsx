import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import {
  getTransactionsByCategory,
  getVisibleTransactions,
} from "~/helpers/transactions";
import { Group, Pagination, ScrollArea, Skeleton, Stack } from "@mantine/core";
import { ITransaction } from "~/models/transaction";
import { IUserSettings } from "~/models/userSettings";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import TransactionCard from "~/components/core/Card/TransactionCard/TransactionCard";
import { useTranslation } from "react-i18next";
import SplitCard, {
  BorderThickness,
} from "~/components/ui/SplitCard/SplitCard";
import { TagsIcon } from "lucide-react";
import BulkActionBar from "~/components/BulkActionBar/BulkActionBar";

const UncategorizedTransactionsCard = (): React.ReactNode => {
  const itemsPerPage = 20;
  const [activePage, setPage] = React.useState(1);
  const [selectedIds, setSelectedIds] = React.useState<Set<string>>(new Set());

  const { t } = useTranslation();
  const { transactionCategories } = useTransactionCategories();
  const { request } = useAuth();

  const transactionsQuery = useQuery({
    queryKey: ["transactions", { getHidden: false }],
    queryFn: async (): Promise<ITransaction[]> => {
      const res: AxiosResponse = await request({
        url: "/api/transaction",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as ITransaction[];
      }

      return [];
    },
  });

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });
      if (res.status === 200) {
        return res.data as IUserSettings;
      }
      return undefined;
    },
  });

  const currency = userSettingsQuery.data?.currency ?? "USD";

  const sortedFilteredTransactions = React.useMemo(
    () =>
      getVisibleTransactions(
        getTransactionsByCategory(transactionsQuery.data ?? [], ""),
      ).sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime()),
    [transactionsQuery.data],
  );

  const currentPageTransactions = sortedFilteredTransactions.slice(
    (activePage - 1) * itemsPerPage,
    (activePage - 1) * itemsPerPage + itemsPerPage,
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

  if (sortedFilteredTransactions.length === 0) {
    return null;
  }

  return (
    <>
      <SplitCard
        w="100%"
        h="100%"
        border={BorderThickness.Thick}
        header={
          <Group gap="0.25rem">
            <TagsIcon color="var(--base-color-text-dimmed)" />
            <PrimaryText size="xl" lh={1}>
              {t("uncategorized_transactions")}
            </PrimaryText>
          </Group>
        }
        elevation={1}
      >
        <Stack
          gap="0.5rem"
          align="center"
          w="100%"
          style={{ flex: 1, minHeight: 0 }}
        >
          {transactionsQuery.isPending ? (
            <Skeleton h="100%" radius="lg" />
          ) : (
            <ScrollArea
              w="100%"
              h="100%"
              p="0.125rem"
              type="auto"
              offsetScrollbars="present"
              style={{ flex: 1, minHeight: 0 }}
            >
              <Stack gap="0.3rem">
                {currentPageTransactions.map((transaction: ITransaction) => (
                  <TransactionCard
                    key={transaction.id}
                    transaction={transaction}
                    categories={transactionCategories}
                    elevation={2}
                    currency={currency}
                    isSelected={selectedIds.has(transaction.id)}
                    onToggleSelect={onToggleSelect}
                  />
                ))}
              </Stack>
            </ScrollArea>
          )}
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
      </SplitCard>
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

export default UncategorizedTransactionsCard;
