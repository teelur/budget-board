import { IValueResponse } from "~/models/value";
import ValueItemContent from "./ValueItemContent/ValueItemContent";
import { useDisclosure } from "@mantine/hooks";
import EditableValueItemContent from "./EditableValueItemContent/EditableValueItemContent";
import ElevatedCard from "~/components/core/Card/ElevatedCard/ElevatedCard";

interface ValueItemProps {
  value: IValueResponse;
  userCurrency: string;
}

const ValueItem = (props: ValueItemProps) => {
  const [isSelected, { open, close }] = useDisclosure(false);
  return (
    <ElevatedCard radius="md">
      {isSelected ? (
        <EditableValueItemContent
          value={props.value}
          userCurrency={props.userCurrency}
          doUnSelect={close}
        />
      ) : (
        <ValueItemContent
          value={props.value}
          userCurrency={props.userCurrency}
          doSelect={open}
        />
      )}
    </ElevatedCard>
  );
};

export default ValueItem;
