import BaseTransactionCard from "./BaseTransactionCard/BaseTransactionCard";
import ElevatedTransactionCard from "./ElevatedTransactionCard/ElevatedTransactionCard";
import SurfaceTransactionCard from "./SurfaceTransactionCard/SurfaceTransactionCard";
import { TransactionCardBaseProps } from "./TransactionCardBase/TransactionCardBase";

export interface TransactionCardProps extends TransactionCardBaseProps {
  elevation?: number;
}

const TransactionCard = ({
  elevation = 0,
  ...props
}: TransactionCardProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      return <BaseTransactionCard {...props} />;
    case 1:
      return <SurfaceTransactionCard {...props} />;
    case 2:
      return <ElevatedTransactionCard {...props} />;
    default:
      throw new Error("Invalid elevation level for TransactionCard");
  }
};

export default TransactionCard;
