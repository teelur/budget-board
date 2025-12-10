import { ActionIcon, Group, Stack } from "@mantine/core";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { INetWorthWidgetLine } from "~/models/widgetSettings";
import NetWorthLineCategory from "./NetWorthLineCategory/NetWorthLineCategory";
import { PlusIcon } from "lucide-react";

export interface INetWorthLineItemProps {
  line: INetWorthWidgetLine;
  index: number;
  updateNetWorthLine: (updatedLine: INetWorthWidgetLine, index: number) => void;
}

const NetWorthLineItem = (props: INetWorthLineItemProps): React.ReactNode => {
  return (
    <Card elevation={1}>
      <Stack gap="0.5rem">
        <Group justify="space-between">
          <PrimaryText size="sm">{props.line.name}</PrimaryText>
          <ActionIcon size="sm">
            <PlusIcon />
          </ActionIcon>
        </Group>
        <Stack gap="0.25rem">
          {props.line.categories.map((category, index) => (
            <NetWorthLineCategory
              key={category.id}
              category={category}
              index={index}
              currentLineName={props.line.name}
              updateNetWorthCategory={(updatedCategory, categoryIndex) => {
                const updatedCategories = [...props.line.categories];
                updatedCategories[categoryIndex] = updatedCategory;
                props.updateNetWorthLine(
                  { ...props.line, categories: updatedCategories },
                  props.index
                );
              }}
            />
          ))}
        </Stack>
      </Stack>
    </Card>
  );
};

export default NetWorthLineItem;
