import surfaceClasses from "~/styles/Surface.module.css";

import React from "react";
import TransactionCardBase, {
  TransactionCardBaseProps,
} from "../TransactionCardBase/TransactionCardBase";

const SurfaceTransactionCard = ({
  ...props
}: TransactionCardBaseProps): React.ReactNode => {
  return (
    <TransactionCardBase
      className={surfaceClasses.root}
      elevation={1}
      {...props}
    />
  );
};

export default SurfaceTransactionCard;
