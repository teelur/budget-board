import EditableBalanceItemContent from "./EditableBalanceItemContent/EditableBalanceItemContent";
import BalanceItemContent from "./BalanceItemContent/BalanceItemContent";
import { IBalanceResponse } from "~/models/balance";
import { useDisclosure } from "@mantine/hooks";
import ElevatedCard from "~/components/Card/ElevatedCard/ElevatedCard";

interface BalanceItemProps {
  balance: IBalanceResponse;
  userCurrency: string;
}

const BalanceItem = (props: BalanceItemProps) => {
  const [isSelected, { open, close }] = useDisclosure(false);
  return (
    <ElevatedCard>
      {isSelected ? (
        <EditableBalanceItemContent
          balance={props.balance}
          userCurrency={props.userCurrency}
          doUnSelect={close}
        />
      ) : (
        <BalanceItemContent
          balance={props.balance}
          userCurrency={props.userCurrency}
          doSelect={open}
        />
      )}
    </ElevatedCard>
  );
};

export default BalanceItem;
