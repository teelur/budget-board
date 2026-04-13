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
  currency: string;
  isSelected?: boolean;
  onToggleSelect?: (id: string) => void;
}

const TransactionCardBase = ({
  transaction,
  categories,
  elevation,
  currency,
  isSelected,
  onToggleSelect,
  ...cardProps
}: TransactionCardBaseProps): React.ReactNode => {
  const selectionMode = onToggleSelect !== undefined;

  return (
    <Card
      w={cardProps.w ?? "100%"}
      p={cardProps.p ?? "0.2rem"}
      {...cardProps}
      style={{ containerType: "inline-size" }}
      onClick={
        selectionMode ? () => onToggleSelect!(transaction.id) : undefined
      }
      elevation={elevation ?? 0}
      className={`${classes.card}${cardProps.className ? ` ${cardProps.className}` : ""}`}
      data-selection-mode={selectionMode ? "true" : undefined}
    >
      {selectionMode ? (
        <Group
          className={classes.selectionGroup}
          data-selected={isSelected ? "true" : "false"}
          wrap="nowrap"
          gap="0.5rem"
          align="center"
        >
          <div className={classes.checkboxWrapper}>
            <Checkbox
              size="xs"
              checked={isSelected ?? false}
              onChange={() => onToggleSelect!(transaction.id)}
              onClick={(e) => e.stopPropagation()}
              elevation={elevation ?? 0}
            />
          </div>
          <TransactionCardContent
            transaction={transaction}
            categories={categories}
            elevation={elevation ?? 0}
            currency={currency}
          />
        </Group>
      ) : (
        <TransactionCardContent
          transaction={transaction}
          categories={categories}
          elevation={elevation ?? 0}
          currency={currency}
        />
      )}
    </Card>
  );
};

export default TransactionCardBase;
