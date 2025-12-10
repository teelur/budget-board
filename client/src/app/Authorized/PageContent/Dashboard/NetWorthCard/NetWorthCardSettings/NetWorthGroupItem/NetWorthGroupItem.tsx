import Card from "~/components/core/Card/Card";
import { INetWorthWidgetLine } from "~/models/widgetSettings";
import NetWorthLineItem from "../NetWorthLineItem/NetWorthLineItem";
import { ActionIcon, Group, Stack } from "@mantine/core";
import { PlusIcon } from "lucide-react";

export interface NetWorthGroupItemProps {
  lines: INetWorthWidgetLine[];
  lineIndexOffset: number;
  updateNetWorthLine: (updatedLine: INetWorthWidgetLine, index: number) => void;
}

const NetWorthGroupItem = (props: NetWorthGroupItemProps): React.ReactNode => {
  return (
    <Card elevation={0}>
      <Stack gap="0.5rem">
        <Group justify="flex-end">
          <ActionIcon size="sm">
            <PlusIcon />
          </ActionIcon>
        </Group>
        <Stack gap="0.25rem">
          {props.lines.map((line, index) => (
            <NetWorthLineItem
              key={line.id}
              line={line}
              index={index}
              updateNetWorthLine={(updatedLine, lineIndex) => {
                props.updateNetWorthLine(
                  updatedLine,
                  props.lineIndexOffset + lineIndex
                );
              }}
            />
          ))}
        </Stack>
      </Stack>
    </Card>
  );
};

export default NetWorthGroupItem;
