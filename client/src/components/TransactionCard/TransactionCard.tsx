import classes from "./TransactionCard.module.css";

import { Card } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { ITransaction } from "~/models/transaction";
import React from "react";
import { ICategory } from "~/models/category";
import EditableTransactionCardContent from "./EditableTransactionCardContent/EditableTransactionCardContent";
import TransactionCardContent from "./TransactionCardContent/TransactionCardContent";

interface TransactionCardProps {
  transaction: ITransaction;
  categories: ICategory[];
  disableEdit?: boolean; // Optional prop to determine if the card is editable
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
      {isSelected && !(props.disableEdit ?? false) ? (
        <EditableTransactionCardContent
          transaction={props.transaction}
          categories={props.categories}
        />
      ) : (
        <TransactionCardContent
          transaction={props.transaction}
          categories={props.categories}
        />
      )}
    </Card>
  );
};

export default TransactionCard;
