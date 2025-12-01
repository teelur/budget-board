import elevatedClasses from "~/styles/Elevated.module.css";

import React from "react";
import TransactionCardBase, {
  TransactionCardBaseProps,
} from "../TransactionCardBase/TransactionCardBase";

interface ElevatedTransactionCardProps extends TransactionCardBaseProps {}

const ElevatedTransactionCard = ({
  ...props
}: ElevatedTransactionCardProps): React.ReactNode => {
  return (
    <TransactionCardBase
      className={elevatedClasses.elevated}
      elevation={2}
      {...props}
    />
  );
};

export default ElevatedTransactionCard;
