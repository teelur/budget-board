import { Group, Stack } from "@mantine/core";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { INetWorthWidgetLine } from "~/models/widgetSettings";

export interface INetWorthLineItemProps {
  line: INetWorthWidgetLine;
}

const NetWorthLineItem = (props: INetWorthLineItemProps): React.ReactNode => {
  return (
    <Card elevation={1}>
      <Group justify="space-between">
        <PrimaryText size="sm">{props.line.name}</PrimaryText>
        <Stack>
          {props.line.categories.map((category) => (
            <PrimaryText key={category.type} size="xs">
              {category.type} - {category.subtype} ({category.value})
            </PrimaryText>
          ))}
        </Stack>
      </Group>
    </Card>
  );
};

export default NetWorthLineItem;
