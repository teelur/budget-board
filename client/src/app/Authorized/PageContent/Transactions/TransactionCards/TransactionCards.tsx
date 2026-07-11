import React from "react";
import { Filters, ITransaction } from "~/models/transaction";
import { Sorts } from "../TransactionsHeader/SortMenu/SortMenuHelpers";
import { SortDirection } from "~/components/SortButton";
import {
  getFilteredTransactions,
  sortTransactions,
} from "~/helpers/transactions";
import { Group, Pagination, Skeleton, Stack } from "@mantine/core";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { useTransactionFilters } from "~/providers/TransactionFiltersProvider/TransactionFiltersProvider";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { useTranslation } from "react-i18next";
import TransactionCard from "~/components/core/Card/TransactionCard/TransactionCard";
import { IUserSettings } from "~/models/userSettings";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { InfoIcon } from "lucide-react";
import { userSettingsQueryKey } from "~/helpers/requests";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";

interface TransactionCardsProps {
  sort: Sorts;
  sortDirection: SortDirection;
  selectedIds: Set<string>;
  onToggleSelect: (id: string) => void;
  onCurrentPageChange: (transactions: ITransaction[]) => void;
}

const TransactionCards = (props: TransactionCardsProps): React.ReactNode => {
  const [page, setPage] = React.useState(1);
  const itemsPerPage = 50;
  const skeletonCount = 10;

  const { t } = useTranslation();
  const { transactionFilters } = useTransactionFilters();
  const { allTransactionCategories: transactionCategories } =
    useTransactionCategories();
  const { request } = useAuth();
  const transactionsQuery = useTransactionsQuery();

  const userSettingsQuery = useQuery({
    queryKey: [userSettingsQueryKey],
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

  const filteredTransactions = getFilteredTransactions(
    transactionsQuery.data ?? [],
    transactionFilters ?? new Filters(),
    transactionCategories,
  );

  const sortedFilteredTransactions = sortTransactions(
    filteredTransactions,
    props.sort,
    props.sortDirection,
  );

  const currentPageItems = sortedFilteredTransactions.slice(
    (page - 1) * itemsPerPage,
    (page - 1) * itemsPerPage + itemsPerPage,
  );

  React.useEffect(() => {
    props.onCurrentPageChange(currentPageItems);
  }, [
    page,
    sortedFilteredTransactions.length,
    currentPageItems,
    props.sort,
    props.sortDirection,
  ]);

  return (
    <Stack gap={"0.5rem"}>
      {transactionsQuery.isPending ? (
        Array.from({ length: skeletonCount }).map((_, index) => (
          <Skeleton key={index} height={40} radius="md" />
        ))
      ) : (
        <Stack gap={"0.3rem"} align="center">
          {sortedFilteredTransactions.length > 0 ? (
            currentPageItems.map((transaction) => (
              <TransactionCard
                key={transaction.id}
                transaction={transaction}
                categories={transactionCategories}
                elevation={1}
                currency={currency}
                isSelected={props.selectedIds.has(transaction.id)}
                onToggleSelect={props.onToggleSelect}
              />
            ))
          ) : (
            <Group justify="center" align="center" gap="0.5rem">
              <InfoIcon size={20} color="var(--base-color-text-dimmed)" />
              <DimmedText size="sm">{t("no_transactions")}</DimmedText>
            </Group>
          )}
        </Stack>
      )}
      <Group justify="center">
        <Pagination
          value={page}
          onChange={setPage}
          total={Math.ceil(filteredTransactions.length / itemsPerPage)}
        />
      </Group>
    </Stack>
  );
};

export default TransactionCards;
