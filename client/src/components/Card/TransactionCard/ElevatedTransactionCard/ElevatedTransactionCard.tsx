import elevatedClasses from "~/styles/Elevated.module.css";

import React from "react";
import TransactionCardBase, {
  TransactionCardBaseProps,
} from "../TransactionCardBase/TransactionCardBase";

const ElevatedTransactionCard = ({
  ...props
}: TransactionCardBaseProps): React.ReactNode => {
  return (
    <TransactionCardBase
      className={elevatedClasses.elevated}
      elevation={2}
      {...props}
    />
  );
};

export default ElevatedTransactionCard;
