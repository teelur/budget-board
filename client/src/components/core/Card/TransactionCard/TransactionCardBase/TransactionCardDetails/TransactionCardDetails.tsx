import classes from "./TransactionCardDetails.module.css";

import { ITransaction } from "~/models/transaction";
import React from "react";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";

interface TransactionCardDetailsProps {
  transaction: ITransaction;
  elevation: number;
}

const borderColorByElevation: Record<number, string> = {
  0: "var(--base-color-border)",
  1: "var(--surface-color-border)",
  2: "var(--elevated-color-border)",
};

const TransactionCardDetails = ({
  transaction,
  elevation,
}: TransactionCardDetailsProps): React.ReactNode => {
  const { t } = useTranslation();
  const notes = transaction.notes.trim();

  return (
    <div
      className={classes.details}
      style={{
        borderTopColor:
          borderColorByElevation[elevation] ?? borderColorByElevation[0],
      }}
    >
      <div className={classes.detailItem}>
        <DimmedText size="xs" elevation={elevation}>
          {t("account")}
        </DimmedText>
        <PrimaryText size="sm" elevation={elevation} className={classes.value}>
          {transaction.accountName.trim() || t("unknown_account")}
        </PrimaryText>
      </div>
      <div className={classes.detailItem}>
        <DimmedText size="xs" elevation={elevation}>
          {t("notes")}
        </DimmedText>
        <PrimaryText size="sm" elevation={elevation} className={classes.value}>
          {notes || t("no_notes")}
        </PrimaryText>
      </div>
    </div>
  );
};

export default TransactionCardDetails;
