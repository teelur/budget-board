import React from "react";
import { Button, Flex, Group } from "@mantine/core";
import { GripVertical } from "lucide-react";
import { useSortable } from "@dnd-kit/react/sortable";
import { RestrictToVerticalAxis } from "@dnd-kit/abstract/modifiers";
import { RestrictToElement } from "@dnd-kit/dom/modifiers";
import { closestCenter } from "@dnd-kit/collision";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";

interface ColumnOrderItemProps {
  fieldKey: string;
  label: string;
  index: number;
  container?: Element;
}

const ColumnOrderItem = (props: ColumnOrderItemProps): React.ReactNode => {
  const { ref, handleRef } = useSortable({
    id: props.fieldKey,
    index: props.index,
    modifiers: [
      ...(props.container
        ? [RestrictToElement.configure({ element: props.container })]
        : []),
      RestrictToVerticalAxis,
    ],
    collisionDetector: closestCenter,
  });

  return (
    <Card ref={ref} elevation={2}>
      <Group w="100%" gap="0.5rem" wrap="nowrap">
        <Flex style={{ alignSelf: "stretch" }}>
          <Button ref={handleRef} h="100%" px={0} w={30} radius="lg">
            <GripVertical size={20} />
          </Button>
        </Flex>
        <PrimaryText size="sm">{props.label}</PrimaryText>
      </Group>
    </Card>
  );
};

export default ColumnOrderItem;
