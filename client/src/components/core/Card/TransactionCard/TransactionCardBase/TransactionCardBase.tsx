import classes from "./TransactionCardBase.module.css";

import { ITransaction } from "~/models/transaction";
import React from "react";
import { ICategory } from "~/models/category";
import TransactionCardContent from "./TransactionCardContent/TransactionCardContent";
import Card, { CardProps } from "../../Card";
import Checkbox from "~/components/core/Checkbox/Checkbox";
import { Group } from "@mantine/core";

export interface TransactionCardBaseProps extends CardProps {
  transaction: ITransaction;
  categories: ICategory[];
  elevation?: number;
  isSelected?: boolean;
  onToggleSelect?: (id: string) => void;
}

const TransactionCardBase = (
  props: TransactionCardBaseProps,
): React.ReactNode => {
  const selectionMode = props.onToggleSelect !== undefined;

  return (
    <Card
      w={props.w ?? "100%"}
      p={props.p ?? "0.2rem"}
      style={{ containerType: "inline-size" }}
      onClick={
        selectionMode
          ? () => props.onToggleSelect!(props.transaction.id)
          : undefined
      }
      elevation={props.elevation ?? 0}
      {...props}
      className={`${classes.card}${props.className ? ` ${props.className}` : ""}`}
      data-selection-mode={selectionMode ? "true" : undefined}
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
