import React from "react";
import { useVirtualizer } from "@tanstack/react-virtual";
import { ITransaction } from "~/models/transaction";
import { Group, Loader, Skeleton, Stack } from "@mantine/core";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { useTranslation } from "react-i18next";
import TransactionCard from "~/components/core/Card/TransactionCard/TransactionCard";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { InfoIcon } from "lucide-react";
import classes from "./TransactionCards.module.css";

interface TransactionCardsProps {
  currentViewTransactions: ITransaction[];
  isQueryPending: boolean;
  selectedIds: Set<string>;
  onToggleSelect: (id: string) => void;
  isViewUpdatePending: boolean;
}

const TransactionCards = (props: TransactionCardsProps): React.ReactNode => {
  const skeletonCount = 10;

  const { t } = useTranslation();
  const { allTransactionCategories } = useTransactionCategories();
  const viewportRef = React.useRef<HTMLDivElement>(null);
  const virtualizer = useVirtualizer({
    count: props.currentViewTransactions.length,
    getScrollElement: () => viewportRef.current,
    estimateSize: () => 56,
    getItemKey: (index) => props.currentViewTransactions[index]?.id ?? index,
    overscan: 5,
  });
  const hasTransactions = props.currentViewTransactions.length > 0;
  const isListPending = props.isQueryPending || props.isViewUpdatePending;

  return (
    <Stack gap="0.5rem" className={classes.container}>
      {props.isQueryPending && !hasTransactions ? (
        Array.from({ length: skeletonCount }).map((_, index) => (
          <Skeleton key={index} height={40} radius="md" />
        ))
      ) : (
        <div
          ref={viewportRef}
          className={classes.viewport}
          aria-busy={isListPending}
        >
          {isListPending && (
<div className={classes.loadingRow}>
              <Loader size="sm" />
            </div>
          )}
          {hasTransactions ? (
            <div
              className={classes.virtualList}
              style={{ height: virtualizer.getTotalSize() }}
            >
              {virtualizer.getVirtualItems().map((virtualItem) => {
                const transaction =
                  props.currentViewTransactions[virtualItem.index];

                if (transaction === undefined) {
                  return null;
                }

                return (
                  <div
                    key={virtualItem.key}
                    data-index={virtualItem.index}
                    ref={virtualizer.measureElement}
                    className={classes.virtualRow}
                    style={{ transform: `translateY(${virtualItem.start}px)` }}
                  >
                    <TransactionCard
                      transaction={transaction}
                      categories={allTransactionCategories}
                      elevation={1}
                      isSelected={props.selectedIds.has(transaction.id)}
                      onToggleSelect={props.onToggleSelect}
                    />
                  </div>
                );
              })}
            </div>
          ) : (
            <Group justify="center" align="center" gap="0.5rem">
              <InfoIcon size={20} color="var(--base-color-text-dimmed)" />
              <DimmedText size="sm">{t("no_transactions")}</DimmedText>
            </Group>
          )}
        </div>
      )}
    </Stack>
  );
};

export default TransactionCards;
