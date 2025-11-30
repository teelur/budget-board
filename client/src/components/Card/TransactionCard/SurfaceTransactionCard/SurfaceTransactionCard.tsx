import cardClasses from "~/styles/Card.module.css";
import surfaceClasses from "~/styles/Surface.module.css";

import React from "react";
import TransactionCardBase, {
  TransactionCardBaseProps,
} from "../TransactionCardBase/TransactionCardBase";

interface SurfaceTransactionCardProps extends TransactionCardBaseProps {}

const SurfaceTransactionCard = ({
  ...props
}: SurfaceTransactionCardProps): React.ReactNode => {
  return (
    <TransactionCardBase
      className={`${cardClasses.card} ${surfaceClasses.root}`}
      elevation={1}
      {...props}
    />
  );
};

export default SurfaceTransactionCard;
