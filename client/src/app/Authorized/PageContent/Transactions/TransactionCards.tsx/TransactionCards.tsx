import React from "react";
import {
  defaultTransactionCategories,
  Filters,
  ITransaction,
} from "~/models/transaction";
import { Sorts } from "../TransactionsHeader/SortMenu/SortMenuHelpers";
import { SortDirection } from "~/components/SortButton";
import {
  getFilteredTransactions,
  sortTransactions,
} from "~/helpers/transactions";
import { Group, Pagination, Skeleton, Stack, Text } from "@mantine/core";
import { AuthContext } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { ICategoryResponse } from "~/models/category";
import TransactionCard from "~/components/TransactionCard/TransactionCard";
import { useTransactionFilters } from "~/providers/TransactionNavigationProvider/TransactionNavigationProvider";

interface TransactionCardsProps {
  sort: Sorts;
  sortDirection: SortDirection;
}

const TransactionCards = (props: TransactionCardsProps): React.ReactNode => {
  const [page, setPage] = React.useState(1);
  const [itemsPerPage, _setItemsPerPage] = React.useState(25);

  const { transactionFilters } = useTransactionFilters();

  const { request } = React.useContext<any>(AuthContext);

  const transactionCategoriesQuery = useQuery({
    queryKey: ["transactionCategories"],
    queryFn: async () => {
      const res = await request({
        url: "/api/transactionCategory",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as ICategoryResponse[];
      }

      return undefined;
    },
  });

  const transactionCategoriesWithCustom = defaultTransactionCategories.concat(
    transactionCategoriesQuery.data ?? []
  );

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
    transactionCategoriesWithCustom
  );

  const sortedFilteredTransactions = sortTransactions(
    filteredTransactions,
    props.sort,
    props.sortDirection
  );

  return (
    <Stack gap={10}>
      {transactionsQuery.isPending || transactionCategoriesQuery.isPending ? (
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
                <TransactionCard
                  key={transaction.id}
                  transaction={transaction}
                  categories={transactionCategoriesWithCustom}
                />
              ))
          ) : (
            <Text fw={600}>No transactions</Text>
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
