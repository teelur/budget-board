import classes from "./UncategorizedTransactionsCard.module.css";

import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import {
  getTransactionsByCategory,
  getVisibleTransactions,
} from "~/helpers/transactions";
import {
  Card,
  Group,
  Pagination,
  ScrollArea,
  Skeleton,
  Stack,
  Title,
} from "@mantine/core";
import { ITransaction } from "~/models/transaction";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import TransactionCardBase from "~/components/Card/TransactionCard/TransactionCardBase/TransactionCardBase";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";

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
    <Card className={classes.root} withBorder radius="md">
      <Stack gap="0.5rem" align="center" w="100%">
        <Group justify="center">
          <Title order={2}>Uncategorized Transactions</Title>
        </Group>
        {transactionsQuery.isPending ? (
          <Skeleton height={350} radius="lg" />
        ) : (
          <ScrollArea.Autosize
            className={classes.scrollArea}
            mah={350}
            type="auto"
            offsetScrollbars
          >
            <Stack className={classes.transactionList}>
              {sortedFilteredTransactions
                .slice(
                  (activePage - 1) * itemsPerPage,
                  (activePage - 1) * itemsPerPage + itemsPerPage
                )
                .map((transaction: ITransaction) => (
                  <TransactionCardBase
                    key={transaction.id}
                    transaction={transaction}
                    categories={transactionCategories}
                    alternateColor
                  />
                ))}
            </Stack>
          </ScrollArea.Autosize>
        )}
        <Pagination
          value={activePage}
          onChange={setPage}
          total={Math.ceil(sortedFilteredTransactions.length / itemsPerPage)}
        />
      </Stack>
    </Card>
  );
};

export default UncategorizedTransactionsCard;
