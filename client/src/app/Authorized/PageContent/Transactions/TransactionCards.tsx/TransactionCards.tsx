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
import SurfaceTransactionCard from "~/components/core/Card/TransactionCard/SurfaceTransactionCard/SurfaceTransactionCard";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";

interface TransactionCardsProps {
  sort: Sorts;
  sortDirection: SortDirection;
}

const TransactionCards = (props: TransactionCardsProps): React.ReactNode => {
  const [page, setPage] = React.useState(1);
  const [itemsPerPage, _setItemsPerPage] = React.useState(25);

  const { transactionFilters } = useTransactionFilters();
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

  const filteredTransactions = getFilteredTransactions(
    transactionsQuery.data ?? [],
    transactionFilters ?? new Filters(),
    transactionCategories
  );

  const sortedFilteredTransactions = sortTransactions(
    filteredTransactions,
    props.sort,
    props.sortDirection
  );

  return (
    <Stack gap={10}>
      {transactionsQuery.isPending ? (
        Array.from({ length: itemsPerPage }).map((_, index) => (
          <Skeleton key={index} height={40} radius="md" />
        ))
      ) : (
        <Stack gap={10} align="center">
          {sortedFilteredTransactions.length > 0 ? (
            sortedFilteredTransactions
              .slice(
                (page - 1) * itemsPerPage,
                (page - 1) * itemsPerPage + itemsPerPage
              )
              .map((transaction) => (
                <SurfaceTransactionCard
                  key={transaction.id}
                  transaction={transaction}
                  categories={transactionCategories}
                />
              ))
          ) : (
            <PrimaryText>No transactions</PrimaryText>
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
