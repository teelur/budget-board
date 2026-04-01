import baseClasses from "~/styles/Base.module.css";

import React from "react";
import TransactionCardBase, {
  TransactionCardBaseProps,
} from "../TransactionCardBase/TransactionCardBase";

const BaseTransactionCard = ({
  ...props
}: TransactionCardBaseProps): React.ReactNode => {
  return (
    <TransactionCardBase
      className={baseClasses.root}
      elevation={0}
      {...props}
    />
  );
};

export default BaseTransactionCard;
