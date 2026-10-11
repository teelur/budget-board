import { Group, Stack } from "@mantine/core";
import { ArrowRight, ArrowRightLeft } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import { ICategory } from "~/models/category";
import { ITransaction } from "~/models/transaction";
import classes from "./TransactionTransferGroup.module.css";
import TransactionCard from "~/components/core/TransactionCard/TransactionCard";
import { BodyText, Card } from "@teelur/budget-board-ui";

interface TransactionTransferGroupProps {
  transactions: [ITransaction, ITransaction];
  categories: ICategory[];
  selectedIds: Set<string>;
  onToggleSelect: (id: string) => void;
}

const TransactionTransferGroup = ({
  transactions,
  categories,
  selectedIds,
  onToggleSelect,
}: TransactionTransferGroupProps): React.ReactNode => {
  const { t } = useTranslation();
  const outgoingTransaction =
    transactions.find((transaction) => transaction.amount < 0) ??
    transactions[0];
  const incomingTransaction =
    transactions.find((transaction) => transaction.amount >= 0) ??
    transactions[1];
  const getAccountName = (transaction: ITransaction) =>
    transaction.accountName.trim() || t("unknown_account");

  return (
    <Card surface="sunken" className={classes.transferGroup}>
      <Stack gap="0.25rem">
        <Group gap="0.5rem" wrap="nowrap">
          <ArrowRightLeft
            className={classes.transferIcon}
            size="1.1rem"
            aria-hidden="true"
          />
          <Stack gap={0}>
            <BodyText tone="primary" size="sm" fw={500}>
              {t("transfer")}
            </BodyText>
            <Group gap="0.35rem" wrap="nowrap">
              <BodyText
                tone="secondary"
                size="xs"
                className={classes.accountName}
              >
                {getAccountName(outgoingTransaction)}
              </BodyText>
              <ArrowRight
                color="var(--bbui-color-primary)"
                size="0.8rem"
                aria-hidden="true"
              />
              <BodyText
                tone="secondary"
                size="xs"
                className={classes.accountName}
              >
                {getAccountName(incomingTransaction)}
              </BodyText>
            </Group>
          </Stack>
        </Group>
        {transactions.map((transaction) => (
          <div key={transaction.id} className={classes.leg}>
            <TransactionCard
              transaction={transaction}
              categories={categories}
              selected={selectedIds.has(transaction.id)}
              onToggleSelect={onToggleSelect}
              p="0.2rem"
            />
          </div>
        ))}
      </Stack>
    </Card>
  );
};

export default TransactionTransferGroup;
