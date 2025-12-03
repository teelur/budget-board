import { useDisclosure } from "@mantine/hooks";
import { ITransaction } from "~/models/transaction";
import React from "react";
import { ICategory } from "~/models/category";
import EditableTransactionCardContent from "./EditableTransactionCardContent/EditableTransactionCardContent";
import TransactionCardContent from "./TransactionCardContent/TransactionCardContent";
import Card, { CardProps } from "../../Card";

export interface TransactionCardBaseProps extends CardProps {
  transaction: ITransaction;
  categories: ICategory[];
  hoverEffect?: boolean;
  elevation?: number;
  disableEdit?: boolean;
}

const TransactionCardBase = (
  props: TransactionCardBaseProps
): React.ReactNode => {
  const [isSelected, { toggle }] = useDisclosure();

  return (
    <Card
      w={props.w ?? "100%"}
      style={{ containerType: "inline-size" }}
      onClick={toggle}
      hoverEffect={props.hoverEffect ?? false}
      elevation={props.elevation ?? 0}
      {...props}
    >
      {isSelected && !(props.disableEdit ?? false) ? (
        <EditableTransactionCardContent
          transaction={props.transaction}
          categories={props.categories}
          elevation={props.elevation ?? 0}
        />
      ) : (
        <TransactionCardContent
          transaction={props.transaction}
          categories={props.categories}
          elevation={props.elevation ?? 0}
        />
      )}
    </Card>
  );
};

export default TransactionCardBase;
