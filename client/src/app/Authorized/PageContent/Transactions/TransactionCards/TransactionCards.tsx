import React from "react";
import { ITransaction } from "~/models/transaction";
import { Group, Skeleton, Stack } from "@mantine/core";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { useTranslation } from "react-i18next";
import TransactionCard from "~/components/core/Card/TransactionCard/TransactionCard";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { InfoIcon } from "lucide-react";

interface TransactionCardsProps {
  currentViewTransactions: ITransaction[];
  isQueryPending: boolean;
  selectedIds: Set<string>;
  onToggleSelect: (id: string) => void;
}

const TransactionCards = (props: TransactionCardsProps): React.ReactNode => {
  const skeletonCount = 10;

  const { t } = useTranslation();
  const { allTransactionCategories } = useTransactionCategories();

  return (
    <Stack gap="0.5rem">
      {props.isQueryPending ? (
        Array.from({ length: skeletonCount }).map((_, index) => (
          <Skeleton key={index} height={40} radius="md" />
        ))
      ) : (
        <Stack gap="0.3rem" align="center">
          {props.currentViewTransactions.length > 0 ? (
            props.currentViewTransactions.map((transaction) => (
              <TransactionCard
                key={transaction.id}
                transaction={transaction}
                categories={allTransactionCategories}
                elevation={1}
                isSelected={props.selectedIds.has(transaction.id)}
                onToggleSelect={props.onToggleSelect}
              />
            ))
          ) : (
            <Group justify="center" align="center" gap="0.5rem">
              <InfoIcon size={20} color="var(--base-color-text-dimmed)" />
              <DimmedText size="sm">{t("no_transactions")}</DimmedText>
            </Group>
          )}
        </Stack>
      )}
    </Stack>
  );
};

export default TransactionCards;
