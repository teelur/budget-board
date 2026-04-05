import classes from "./TransactionCardBase.module.css";

import { useDisclosure } from "@mantine/hooks";
import { ITransaction } from "~/models/transaction";
import React from "react";
import { ICategory } from "~/models/category";
import EditableTransactionCardContent from "./EditableTransactionCardContent/EditableTransactionCardContent";
import TransactionCardContent from "./TransactionCardContent/TransactionCardContent";
import Card, { CardProps } from "../../Card";
import Checkbox from "~/components/core/Checkbox/Checkbox";
import { Group } from "@mantine/core";

export interface TransactionCardBaseProps extends CardProps {
  transaction: ITransaction;
  categories: ICategory[];
  hoverEffect?: boolean;
  elevation?: number;
  disableEdit?: boolean;
  isSelected?: boolean;
  onToggleSelect?: (id: string) => void;
}

const TransactionCardBase = (
  props: TransactionCardBaseProps,
): React.ReactNode => {
  const [isEditOpen, { toggle }] = useDisclosure();

  const selectionMode = props.onToggleSelect !== undefined;

  const handleCardClick = () => {
    if (selectionMode) {
      props.onToggleSelect!(props.transaction.id);
    } else {
      toggle();
    }
  };

  return (
    <Card
      w={props.w ?? "100%"}
      p={props.p ?? "0.2rem"}
      style={{ containerType: "inline-size" }}
      onClick={handleCardClick}
      hoverEffect={props.hoverEffect ?? false}
      elevation={props.elevation ?? 0}
      {...props}
      className={`${classes.card}${props.className ? ` ${props.className}` : ""}`}
    >
      {selectionMode ? (
        <Group
          className={classes.selectionGroup}
          data-selected={props.isSelected ? "true" : "false"}
          wrap="nowrap"
          gap="0.5rem"
          align="center"
        >
          <div className={classes.checkboxWrapper}>
            <Checkbox
              size="xs"
              checked={props.isSelected ?? false}
              onChange={() => props.onToggleSelect!(props.transaction.id)}
              onClick={(e) => e.stopPropagation()}
              elevation={props.elevation ?? 0}
            />
          </div>
          <TransactionCardContent
            transaction={props.transaction}
            categories={props.categories}
            elevation={props.elevation ?? 0}
          />
        </Group>
      ) : isEditOpen && !(props.disableEdit ?? false) ? (
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
