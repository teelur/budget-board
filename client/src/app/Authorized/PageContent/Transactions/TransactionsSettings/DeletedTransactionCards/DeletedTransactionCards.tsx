import { Group, Pagination, Stack } from "@mantine/core";
import React from "react";
import { ITransaction } from "~/models/transaction";
import DeletedTransactionsCard from "./DeletedTransactionCard/DeletedTransactionsCard";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";

interface DeletedTransactionCardsProps {
  transactions: ITransaction[];
}

const DeletedTransactionCards = (
  props: DeletedTransactionCardsProps,
): React.ReactNode => {
  const [page, setPage] = React.useState(1);
  const [itemsPerPage, _setItemsPerPage] = React.useState(15);

  const { t } = useTranslation();

  return (
    <Stack gap="0.5rem">
      <Stack gap="0.5rem">
        {props.transactions.length > 0 ? (
          props.transactions
            .slice(
              (page - 1) * itemsPerPage,
              (page - 1) * itemsPerPage + itemsPerPage,
            )
            .map((transaction) => (
              <DeletedTransactionsCard
                key={transaction.id}
                deletedTransaction={transaction}
              />
            ))
        ) : (
          <PrimaryText size="xs">{t("no_transactions")}</PrimaryText>
        )}
      </Stack>
      <Group justify="center">
        <Pagination
          value={page}
          onChange={setPage}
          total={Math.ceil(props.transactions.length / itemsPerPage)}
        />
      </Group>
    </Stack>
  );
};

export default DeletedTransactionCards;
