import { Group, Pagination, Stack } from "@mantine/core";
import { ITransaction } from "~/models/transaction";
import { ICategory } from "~/models/category";
import React from "react";
import TransactionCard from "~/components/core/Card/TransactionCard/TransactionCard";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";

interface TransactionCardsProps {
  transactions: ITransaction[];
  categories: ICategory[];
}

const TransactionCards = (props: TransactionCardsProps): React.ReactNode => {
  const [page, setPage] = React.useState(1);
  const itemsPerPage = 5;

  const { t } = useTranslation();

  const paginatedItems: ITransaction[] = props.transactions.slice(
    (page - 1) * itemsPerPage,
    page * itemsPerPage
  );

  if (props.transactions.length === 0) {
    return (
      <Group justify="center">
        <DimmedText size="sm">{t("no_transactions")}</DimmedText>
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
