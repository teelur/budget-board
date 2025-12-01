import surfaceClasses from "~/styles/Surface.module.css";

import React from "react";
import TransactionCardBase, {
  TransactionCardBaseProps,
} from "../TransactionCardBase/TransactionCardBase";

interface SurfaceTransactionCardProps extends TransactionCardBaseProps {}

// TODO: Convert to generic these are fucked.
const SurfaceTransactionCard = ({
  ...props
}: SurfaceTransactionCardProps): React.ReactNode => {
  return (
    <TransactionCardBase
      className={surfaceClasses.root}
      elevation={1}
      {...props}
    />
  );
};

export default SurfaceTransactionCard;
