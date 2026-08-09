import classes from "./TransactionCardBase.module.css";

import { ITransaction } from "~/models/transaction";
import React from "react";
import { ICategory } from "~/models/category";
import TransactionCardContent from "./TransactionCardContent/TransactionCardContent";
import TransactionCardDetails from "./TransactionCardDetails/TransactionCardDetails";
import Card, { CardProps } from "../../Card";
import Checkbox from "~/components/core/Checkbox/Checkbox";
import { ActionIcon, Collapse, Group } from "@mantine/core";
import { ChevronDown } from "lucide-react";
import { useTranslation } from "react-i18next";

export interface TransactionCardBaseProps extends CardProps {
  transaction: ITransaction;
  categories: ICategory[];
  elevation?: number;
  isSelected?: boolean;
  onToggleSelect?: (id: string) => void;
}

const TransactionCardBase = ({
  transaction,
  categories,
  elevation,
  isSelected,
  onToggleSelect,
  ...cardProps
}: TransactionCardBaseProps): React.ReactNode => {
  const { t } = useTranslation();
  const [isDetailsOpen, setIsDetailsOpen] = React.useState(false);
  const selectionMode = onToggleSelect !== undefined;
  const detailsId = React.useId();
  const detailsLabel = t(
    isDetailsOpen
      ? "collapse_transaction_details"
      : "expand_transaction_details",
  );

  const transactionContent = (
    <div className={classes.contentWrapper}>
      <TransactionCardContent
        transaction={transaction}
        categories={categories}
        elevation={elevation ?? 0}
      />
    </div>
  );

  const detailsToggle = (
    <ActionIcon
      variant="subtle"
      size="sm"
      aria-label={detailsLabel}
      title={detailsLabel}
      aria-expanded={isDetailsOpen}
      aria-controls={detailsId}
      onClick={(event) => {
        event.stopPropagation();
        setIsDetailsOpen((prev) => !prev);
      }}
    >
      <ChevronDown
        size="1rem"
        className={`${classes.chevron}${isDetailsOpen ? ` ${classes.chevronOpen}` : ""}`}
      />
    </ActionIcon>
  );

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
        <div className={classes.header}>
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
                onClick={(event) => event.stopPropagation()}
                elevation={elevation ?? 0}
              />
            </div>
            {transactionContent}
          </Group>
          {detailsToggle}
        </div>
      ) : (
        <div className={classes.header}>
          {transactionContent}
          {detailsToggle}
        </div>
      )}
      <Collapse id={detailsId} expanded={isDetailsOpen}>
        <TransactionCardDetails
          transaction={transaction}
          elevation={elevation ?? 0}
        />
      </Collapse>
    </Card>
  );
};

export default TransactionCardBase;
