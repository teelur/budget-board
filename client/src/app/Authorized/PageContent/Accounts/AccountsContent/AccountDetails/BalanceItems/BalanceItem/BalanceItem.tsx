import { Card } from "@mantine/core";
import EditableBalanceItemContent from "./EditableBalanceItemContent/EditableBalanceItemContent";
import BalanceItemContent from "./BalanceItemContent/BalanceItemContent";
import { IBalanceResponse } from "~/models/balance";
import { useDisclosure } from "@mantine/hooks";

interface BalanceItemProps {
  balance: IBalanceResponse;
  userCurrency: string;
}

const BalanceItem = (props: BalanceItemProps) => {
  const [isSelected, { open, close }] = useDisclosure(false);
  return (
    <Card radius="md" p="0.5rem">
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
    </Card>
  );
};

export default BalanceItem;
