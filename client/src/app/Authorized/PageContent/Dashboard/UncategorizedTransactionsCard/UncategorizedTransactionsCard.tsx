import classes from "./UncategorizedTransactionsCard.module.css";

import { AuthContext } from "~/components/AuthProvider/AuthProvider";
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
import {
  defaultTransactionCategories,
  ITransaction,
} from "~/models/transaction";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { ICategoryResponse } from "~/models/category";
import TransactionCard from "~/components/TransactionCard/TransactionCard";

const UncategorizedTransactionsCard = (): React.ReactNode => {
  const itemsPerPage = 20;
  const [activePage, setPage] = React.useState(1);

  const { request } = React.useContext<any>(AuthContext);
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
        {transactionsQuery.isPending || transactionCategoriesQuery.isPending ? (
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
                  <TransactionCard
                    key={transaction.id}
                    transaction={transaction}
                    categories={transactionCategoriesWithCustom}
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
