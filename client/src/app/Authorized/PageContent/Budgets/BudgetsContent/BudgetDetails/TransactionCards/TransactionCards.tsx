import { Group, Pagination, Stack } from "@mantine/core";
import { ITransaction } from "~/models/transaction";
import { ICategory } from "~/models/category";
import React from "react";
import TransactionCard from "~/components/Card/TransactionCard/TransactionCard";
import DimmedText from "~/components/Text/DimmedText/DimmedText";

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

  if (props.transactions.length === 0) {
    return (
      <Group justify="center">
        <DimmedText size="sm">No transactions found.</DimmedText>
      </Group>
    );
  }

  return (
    <Stack gap="0.5rem">
      {paginatedItems.map((transaction) => (
        <TransactionCard
          key={transaction.id}
          transaction={transaction}
          categories={props.categories}
          elevation={2}
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
