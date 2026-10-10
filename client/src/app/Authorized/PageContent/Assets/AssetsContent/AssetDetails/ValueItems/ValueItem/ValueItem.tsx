import { Card } from "@teelur/budget-board-ui";
import { IValueResponse } from "~/models/value";
import ValueItemContent from "./ValueItemContent/ValueItemContent";
import { useDisclosure } from "@mantine/hooks";
import EditableValueItemContent from "./EditableValueItemContent/EditableValueItemContent";

interface ValueItemProps {
  value: IValueResponse;
}

const ValueItem = (props: ValueItemProps) => {
  const [isSelected, { open, close }] = useDisclosure(false);
  return (
    <Card>
      {isSelected ? (
        <EditableValueItemContent value={props.value} doUnSelect={close} />
      ) : (
        <ValueItemContent value={props.value} doSelect={open} />
      )}
    </Card>
  );
};

export default ValueItem;
