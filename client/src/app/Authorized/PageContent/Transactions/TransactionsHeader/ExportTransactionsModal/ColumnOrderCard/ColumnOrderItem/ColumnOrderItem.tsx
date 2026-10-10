import React from "react";
import { Flex, Group } from "@mantine/core";
import { BodyText, Button, Card } from "@teelur/budget-board-ui";
import { GripVertical } from "lucide-react";
import { useSortable } from "@dnd-kit/react/sortable";
import { RestrictToVerticalAxis } from "@dnd-kit/abstract/modifiers";
import { RestrictToElement } from "@dnd-kit/dom/modifiers";
import { closestCenter } from "@dnd-kit/collision";

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
    <Card ref={ref}>
      <Group w="100%" gap="0.5rem" wrap="nowrap">
        <Flex ref={handleRef} style={{ alignSelf: "stretch" }}>
          <Button variant="filled" color="primary" h="100%" px={0} w={30}>
            <GripVertical size={20} />
          </Button>
        </Flex>
        <BodyText component="span" tone="primary" size="sm" fw={500}>
          {props.label}
        </BodyText>
      </Group>
    </Card>
  );
};

export default ColumnOrderItem;
