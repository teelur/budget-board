import EditableBalanceItemContent from "./EditableBalanceItemContent/EditableBalanceItemContent";
import BalanceItemContent from "./BalanceItemContent/BalanceItemContent";
import { IBalanceResponse } from "~/models/balance";
import { useDisclosure } from "@mantine/hooks";
import { Card } from "@teelur/budget-board-ui";

interface BalanceItemProps {
  balance: IBalanceResponse;
}

const BalanceItem = (props: BalanceItemProps) => {
  const [isSelected, { open, close }] = useDisclosure(false);
  return (
    <Card>
      {isSelected ? (
        <EditableBalanceItemContent
          balance={props.balance}
          doUnSelect={close}
        />
      ) : (
        <BalanceItemContent balance={props.balance} doSelect={open} />
      )}
    </Card>
  );
};

export default BalanceItem;
