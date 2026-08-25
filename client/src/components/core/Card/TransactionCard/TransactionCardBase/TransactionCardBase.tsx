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

const hoverOpenDelay = 300;

export interface TransactionCardBaseProps extends CardProps {
  transaction: ITransaction;
  categories: ICategory[];
  elevation?: number;
  isSelected?: boolean;
  onToggleSelect?: (id: string) => void;
  showTransactionLink?: boolean;
}

const TransactionCardBase = ({
  transaction,
  categories,
  elevation,
  isSelected,
  onToggleSelect,
  showTransactionLink,
  ...cardProps
}: TransactionCardBaseProps): React.ReactNode => {
  const { t } = useTranslation();
  const [isDetailsOpen, setIsDetailsOpen] = React.useState(false);
  const [isHovered, setIsHovered] = React.useState(false);
  const hoverTimeoutRef = React.useRef<ReturnType<typeof setTimeout> | null>(
    null,
  );
  const selectionMode = onToggleSelect !== undefined;
  const hoverEffect = cardProps.hoverEffect ?? selectionMode;
  const isDetailsExpanded = isDetailsOpen || (hoverEffect && isHovered);
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
        if (!hoverEffect) {
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
        if (!hoverEffect) {
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
        hoverEffect={hoverEffect}
        elevation={elevation ?? 0}
        className={cardProps.className}
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
        <Collapse id={detailsId} expanded={isDetailsExpanded}>
          <TransactionCardDetails
            transaction={transaction}
            elevation={elevation ?? 0}
            showTransactionLink={showTransactionLink}
          />
        </Collapse>
      </Card>
    </div>
  );
};

export default TransactionCardBase;
