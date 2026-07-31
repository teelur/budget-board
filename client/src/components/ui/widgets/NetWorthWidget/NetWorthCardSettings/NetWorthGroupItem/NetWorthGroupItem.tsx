import Card from "~/components/core/Card/Card";
import {
  INetWorthWidgetGroup,
  INetWorthWidgetLine,
} from "~/models/widgetSettings";
import NetWorthLineItem from "./NetWorthLineItem/NetWorthLineItem";
import {
  ActionIcon,
  Button,
  Flex,
  Group,
  LoadingOverlay,
  Stack,
} from "@mantine/core";
import { GripVertical, PlusIcon } from "lucide-react";
import {
  INetWorthWidgetLineCreateRequest,
  INetWorthWidgetLineReorderRequest,
} from "~/models/netWorthWidgetConfiguration";
import React from "react";
import { DragDropProvider } from "@dnd-kit/react";
import { move } from "@dnd-kit/helpers";
import { useDidUpdate } from "@mantine/hooks";
import { useSortable } from "@dnd-kit/react/sortable";
import { RestrictToVerticalAxis } from "@dnd-kit/abstract/modifiers";
import { RestrictToElement } from "@dnd-kit/dom/modifiers";
import { closestCorners } from "@dnd-kit/collision";
import { useCreateNetWorthWidgetLineMutation } from "~/hooks/mutations/netWorthWidgetLine/useCreateNetWorthWidgetLineMutation";
import { useReorderNetWorthWidgetLineMutation } from "~/hooks/mutations/netWorthWidgetLine/useReorderNetWorthWidgetLineMutation";

export interface NetWorthGroupItemProps {
  group: INetWorthWidgetGroup;
  isSortable: boolean;
  container: Element;
  settingsId: string;
  allLines: INetWorthWidgetLine[];
}

const NetWorthGroupItem = (props: NetWorthGroupItemProps): React.ReactNode => {
  const createNetWorthWidgetLineMutation =
    useCreateNetWorthWidgetLineMutation();
  const reorderNetWorthWidgetLineMutation =
    useReorderNetWorthWidgetLineMutation();

  const [sortedLineItems, setSortedLineItems] = React.useState<
    INetWorthWidgetLine[]
  >([]);

  const linesStackRef = React.useRef<HTMLDivElement>(null);

  React.useEffect(() => {
    setSortedLineItems(
      props.group.lines
        .slice()
        .sort((a, b) => a.index - b.index)
        .map((line, index) => ({
          ...line,
          index,
        })),
    );
  }, [props.group.lines]);

  useDidUpdate(() => {
    if (!props.isSortable) {
      const orderedLines: string[] = sortedLineItems.map((line) => line.id);

      reorderNetWorthWidgetLineMutation.mutate({
        widgetSettingsId: props.settingsId,
        groupId: props.group.id,
        orderedLineIds: orderedLines,
      } as INetWorthWidgetLineReorderRequest);
    }
  }, [props.isSortable]);

  const { ref, handleRef } = useSortable({
    id: props.group.id,
    index: props.group.index,
    modifiers: [
      RestrictToElement.configure({ element: props.container }),
      RestrictToVerticalAxis,
    ],
    collisionDetector: closestCorners,
  });

  return (
    <Card ref={props.isSortable ? ref : undefined} elevation={0}>
      <LoadingOverlay visible={reorderNetWorthWidgetLineMutation.isPending} />
      <Group gap="0.5rem">
        {props.isSortable && (
          <Flex ref={handleRef} style={{ alignSelf: "stretch" }}>
            <Button h="100%" px={0} w={{ base: 25, xs: 30 }} radius="lg">
              <GripVertical size={25} />
            </Button>
          </Flex>
        )}
        <Stack flex="1 0 auto" gap="0.5rem">
          <Group justify="flex-end">
            <ActionIcon
              size="sm"
              loading={createNetWorthWidgetLineMutation.isPending}
              onClick={async () =>
                await createNetWorthWidgetLineMutation.mutateAsync({
                  name: "",
                  group: props.group.index,
                  index: props.group.lines.length,
                  widgetSettingsId: props.settingsId,
                } as INetWorthWidgetLineCreateRequest)
              }
            >
              <PlusIcon />
            </ActionIcon>
          </Group>
          <DragDropProvider
            onDragEnd={(event) => {
              const updatedList = move(sortedLineItems, event).map(
                (line, index) => ({
                  ...line,
                  index,
                }),
              );

              setSortedLineItems(updatedList);
            }}
          >
            <Stack ref={linesStackRef} gap="0.25rem">
              {sortedLineItems.map((line) => (
                <NetWorthLineItem
                  key={line.id}
                  container={linesStackRef.current as Element}
                  line={line}
                  groupIndex={props.group.index}
                  lines={props.allLines}
                  settingsId={props.settingsId}
                  isSortable={props.isSortable}
                />
              ))}
            </Stack>
          </DragDropProvider>
        </Stack>
      </Group>
    </Card>
  );
};

export default NetWorthGroupItem;
