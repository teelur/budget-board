import { Group, Stack } from "@mantine/core";
import { ArrowRight, ArrowRightLeft } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import { ICategory } from "~/models/category";
import { ITransaction } from "~/models/transaction";
import TransactionCard from "~/components/core/Card/TransactionCard/TransactionCard";
import TransactionLinkDialog from "~/components/core/Card/TransactionCard/TransactionCardBase/TransactionCardDetails/TransactionLinkDialog";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import classes from "./TransactionTransferGroup.module.css";

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
    <Stack className={classes.transferGroup} gap="0.25rem">
      <div className={classes.summary}>
        <Group className={classes.summaryMain} gap="0.5rem" wrap="nowrap">
          <ArrowRightLeft
            className={classes.transferIcon}
            size="1.1rem"
            aria-hidden="true"
          />
          <Stack className={classes.summaryContent} gap={0}>
            <PrimaryText size="sm">{t("transfer")}</PrimaryText>
            <Group
              className={classes.accountSummary}
              gap="0.35rem"
              wrap="nowrap"
            >
              <PrimaryText size="xs" className={classes.accountName}>
                {getAccountName(outgoingTransaction)}
              </PrimaryText>
              <ArrowRight size="0.8rem" aria-hidden="true" />
              <PrimaryText size="xs" className={classes.accountName}>
                {getAccountName(incomingTransaction)}
              </PrimaryText>
            </Group>
          </Stack>
        </Group>
        <div className={classes.summaryAction}>
          <TransactionLinkDialog
            transaction={transactions[0]}
            elevation={1}
            showLinkedDetails={false}
          />
        </div>
      </div>
      {transactions.map((transaction) => (
        <div key={transaction.id} className={classes.leg}>
          <TransactionCard
            transaction={transaction}
            categories={categories}
            elevation={1}
            isSelected={selectedIds.has(transaction.id)}
            onToggleSelect={onToggleSelect}
            p="0.2rem"
            showTransactionLink={false}
          />
        </div>
      ))}
    </Stack>
  );
};

export default TransactionTransferGroup;
