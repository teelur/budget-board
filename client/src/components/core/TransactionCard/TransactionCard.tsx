import classes from "./TransactionCard.module.css";

import { Collapse, Group } from "@mantine/core";
import { ActionIcon, Card, CardProps, Checkbox } from "@teelur/budget-board-ui";
import { ITransaction } from "~/models/transaction";
import React from "react";
import { ICategory } from "~/models/category";
import TransactionCardContent from "./TransactionCardContent/TransactionCardContent";
import TransactionCardDetails from "./TransactionCardDetails/TransactionCardDetails";
import { ChevronDown } from "lucide-react";
import { useTranslation } from "react-i18next";

const hoverOpenDelay = 400;

export interface TransactionCardProps extends CardProps {
  transaction: ITransaction;
  categories: ICategory[];
  onToggleSelect?: (id: string) => void;
}

const TransactionCard = ({
  transaction,
  categories,
  onToggleSelect,
  ...cardProps
}: TransactionCardProps): React.ReactNode => {
  const { t } = useTranslation();
  const [isDetailsOpen, setIsDetailsOpen] = React.useState(false);
  const [isHovered, setIsHovered] = React.useState(false);
  const hoverTimeoutRef = React.useRef<ReturnType<typeof setTimeout> | null>(
    null,
  );
  const selectionMode = onToggleSelect !== undefined;
  const hoverable = cardProps.hoverable ?? selectionMode;
  const isDetailsExpanded = isDetailsOpen || (hoverable && isHovered);
  const detailsId = React.useId();

  React.useEffect(() => {
    return () => {
      if (hoverTimeoutRef.current !== null) {
        clearTimeout(hoverTimeoutRef.current);
      }
    };
  }, []);
  const detailsLabel = t(
    isDetailsExpanded
      ? "collapse_transaction_details"
      : "expand_transaction_details",
  );

  const transactionContent = (
    <div className={classes.contentWrapper}>
      <TransactionCardContent
        transaction={transaction}
        categories={categories}
      />
    </div>
  );

  const detailsToggle = (
    <ActionIcon
      variant="ghost"
      color="primary"
      size="compact-xs"
      aria-label={detailsLabel}
      title={detailsLabel}
      aria-expanded={isDetailsExpanded}
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
    <div
      onMouseEnter={() => {
        if (!hoverable) {
          return;
        }

        if (hoverTimeoutRef.current !== null) {
          clearTimeout(hoverTimeoutRef.current);
        }

        hoverTimeoutRef.current = setTimeout(() => {
          setIsHovered(true);
          hoverTimeoutRef.current = null;
        }, hoverOpenDelay);
      }}
      onMouseLeave={() => {
        if (!hoverable) {
          return;
        }

        if (hoverTimeoutRef.current !== null) {
          clearTimeout(hoverTimeoutRef.current);
          hoverTimeoutRef.current = null;
        }

        setIsHovered(false);
      }}
    >
      <Card
        w={cardProps.w ?? "100%"}
        p={cardProps.p ?? "0.2rem"}
        {...cardProps}
        style={{ containerType: "inline-size" }}
        onClick={
          selectionMode ? () => onToggleSelect!(transaction.id) : undefined
        }
        hoverable={hoverable}
      >
        {selectionMode ? (
          <Group gap="0.5rem" wrap="nowrap">
            <Group
              className={classes.selectionGroup}
              data-selected={cardProps.selected ? "true" : "false"}
              wrap="nowrap"
              gap="0.5rem"
              align="center"
            >
              <div className={classes.checkboxWrapper}>
                <Checkbox
                  size="xs"
                  checked={cardProps.selected ?? false}
                  onChange={() => onToggleSelect!(transaction.id)}
                  onClick={(event) => event.stopPropagation()}
                />
              </div>
              {transactionContent}
            </Group>
            {detailsToggle}
          </Group>
        ) : (
          <Group gap="0.5rem">
            {transactionContent}
            {detailsToggle}
          </Group>
        )}
        <Collapse id={detailsId} expanded={isDetailsExpanded}>
          <TransactionCardDetails transaction={transaction} />
        </Collapse>
      </Card>
    </div>
  );
};

export default TransactionCard;
