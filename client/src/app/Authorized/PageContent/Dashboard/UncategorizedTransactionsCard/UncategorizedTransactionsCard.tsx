import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import {
  getTransactionsByCategory,
  getVisibleTransactions,
} from "~/helpers/transactions";
import { Pagination, ScrollArea, Skeleton, Stack } from "@mantine/core";
import { ITransaction } from "~/models/transaction";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import Card from "~/components/Card/Card";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";
import TransactionCard from "~/components/Card/TransactionCard/TransactionCard";

const UncategorizedTransactionsCard = (): React.ReactNode => {
  const itemsPerPage = 20;
  const [activePage, setPage] = React.useState(1);

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

  const sortedFilteredTransactions = React.useMemo(
    () =>
      getVisibleTransactions(
        getTransactionsByCategory(transactionsQuery.data ?? [], "")
      ).sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime()),
    [transactionsQuery.data]
  );

  if (sortedFilteredTransactions.length === 0) {
    return null;
  }

  return (
    <Card w="100%" elevation={1}>
      <Stack gap="0.5rem" align="center" w="100%">
        <PrimaryText size="xl">Uncategorized Transactions</PrimaryText>
        {transactionsQuery.isPending ? (
          <Skeleton height={350} radius="lg" />
        ) : (
          <ScrollArea.Autosize
            w="100%"
            p="0.125rem"
            mah={350}
            type="auto"
            offsetScrollbars
          >
            <Stack gap="0.5rem">
              {sortedFilteredTransactions
                .slice(
                  (activePage - 1) * itemsPerPage,
                  (activePage - 1) * itemsPerPage + itemsPerPage
                )
                .map((transaction: ITransaction) => (
                  <TransactionCard
                    key={transaction.id}
                    transaction={transaction}
                    categories={transactionCategories}
                    hoverEffect
                    elevation={2}
                  />
                ))}
            </Stack>
          </ScrollArea.Autosize>
        )}
        {sortedFilteredTransactions.length > itemsPerPage && (
          <Pagination
            value={activePage}
            onChange={setPage}
            total={Math.ceil(sortedFilteredTransactions.length / itemsPerPage)}
          />
        )}
      </Stack>
    </Card>
  );
};

export default UncategorizedTransactionsCard;
