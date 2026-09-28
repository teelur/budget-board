import EditableBalanceItemContent from "./EditableBalanceItemContent/EditableBalanceItemContent";
import BalanceItemContent from "./BalanceItemContent/BalanceItemContent";
import { IBalanceResponse } from "~/models/balance";
import { useDisclosure } from "@mantine/hooks";
import ElevatedCard from "~/components/core/Card/ElevatedCard/ElevatedCard";

interface BalanceItemProps {
  balance: IBalanceResponse;
}

const BalanceItem = (props: BalanceItemProps) => {
  const [isSelected, { open, close }] = useDisclosure(false);
  return (
    <ElevatedCard>
      {isSelected ? (
        <EditableBalanceItemContent
          balance={props.balance}
          doUnSelect={close}
        />
      ) : (
        <BalanceItemContent
          balance={props.balance}
          doSelect={open}
        />
      )}
    </ElevatedCard>
  );
};

export default BalanceItem;
