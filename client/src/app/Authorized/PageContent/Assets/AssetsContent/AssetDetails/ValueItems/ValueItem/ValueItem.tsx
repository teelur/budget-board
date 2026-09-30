import { IValueResponse } from "~/models/value";
import ValueItemContent from "./ValueItemContent/ValueItemContent";
import { useDisclosure } from "@mantine/hooks";
import EditableValueItemContent from "./EditableValueItemContent/EditableValueItemContent";
import ElevatedCard from "~/components/core/Card/ElevatedCard/ElevatedCard";

interface ValueItemProps {
  value: IValueResponse;
}

const ValueItem = (props: ValueItemProps) => {
  const [isSelected, { open, close }] = useDisclosure(false);
  return (
    <ElevatedCard radius="md">
      {isSelected ? (
        <EditableValueItemContent value={props.value} doUnSelect={close} />
      ) : (
        <ValueItemContent value={props.value} doSelect={open} />
      )}
    </ElevatedCard>
  );
};

export default ValueItem;
