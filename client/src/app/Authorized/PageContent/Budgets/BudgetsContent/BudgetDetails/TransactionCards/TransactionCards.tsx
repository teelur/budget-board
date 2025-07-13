import { Group, Pagination, Stack } from "@mantine/core";
import TransactionCard from "./TransactionCard/TransactionCard";
import { ITransaction } from "~/models/transaction";
import { ICategory } from "~/models/category";
import React from "react";

interface TransactionCardsProps {
  transactions: ITransaction[];
  categories: ICategory[];
}

const TransactionCards = (props: TransactionCardsProps): React.ReactNode => {
  const [page, setPage] = React.useState(1);
  const itemsPerPage = 5;

  const paginatedItems: ITransaction[] = props.transactions.slice(
    (page - 1) * itemsPerPage,
    page * itemsPerPage
  );

  return (
    <Stack>
      {paginatedItems.map((transaction) => (
        <TransactionCard
          key={transaction.id}
          transaction={transaction}
          categories={props.categories}
        />
      ))}
      {props.transactions.length > itemsPerPage && (
        <Group justify="center">
          <Pagination
            total={Math.ceil(props.transactions.length / itemsPerPage)}
            value={page}
            onChange={setPage}
          />
        </Group>
      )}
    </Stack>
  );
};

export default TransactionCards;
