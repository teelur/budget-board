import classes from "./TransactionCard.module.css";

import { Card } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { ITransaction } from "~/models/transaction";
import React from "react";
import { ICategory } from "~/models/category";
import UnselectedTransactionCard from "./UnselectedTransactionCard/UnselectedTransactionCard";
import SelectedTransactionCard from "./SelectedTransactionCard/SelectedTransactionCard";

interface TransactionCardProps {
  transaction: ITransaction;
  categories: ICategory[];
}

const TransactionCard = (props: TransactionCardProps): React.ReactNode => {
  const [isSelected, { toggle }] = useDisclosure();

  return (
    <Card
      className={classes.card}
      onClick={toggle}
      radius="md"
      withBorder={isSelected}
      bg={isSelected ? "var(--mantine-primary-color-light)" : ""}
      shadow="md"
    >
      {isSelected ? (
        <SelectedTransactionCard
          transaction={props.transaction}
          categories={props.categories}
        />
      ) : (
        <UnselectedTransactionCard
          transaction={props.transaction}
          categories={props.categories}
        />
      )}
    </Card>
  );
};

export default TransactionCard;
