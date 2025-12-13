import Card from "~/components/core/Card/Card";
import {
  INetWorthWidgetGroup,
  INetWorthWidgetLine,
} from "~/models/widgetSettings";
import NetWorthLineItem from "../NetWorthLineItem/NetWorthLineItem";
import {
  ActionIcon,
  Button,
  Flex,
  Group,
  LoadingOverlay,
  Stack,
} from "@mantine/core";
import { GripVertical, PlusIcon } from "lucide-react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { notifications } from "@mantine/notifications";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
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

export interface NetWorthGroupItemProps {
  group: INetWorthWidgetGroup;
  isSortable: boolean;
  container: Element;
  settingsId: string;
}

const NetWorthGroupItem = (props: NetWorthGroupItemProps): React.ReactNode => {
  const [sortedLineItems, setSortedLineItems] = React.useState<
    INetWorthWidgetLine[]
  >([]);

  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doCreateLine = useMutation({
    mutationFn: async (newLine: INetWorthWidgetLineCreateRequest) =>
      await request({
        url: `/api/netWorthWidgetLine`,
        method: "POST",
        data: newLine,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["widgetSettings"] });

      notifications.show({
        color: "var(--button-color-confirm)",
        message: "Net worth settings updated successfully.",
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  const doReorderLines = useMutation({
    mutationFn: async (reorderRequest: INetWorthWidgetLineReorderRequest) =>
      await request({
        url: `/api/netWorthWidgetLine/reorder`,
        method: "POST",
        data: reorderRequest,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["widgetSettings"] });
      notifications.show({
        color: "var(--button-color-confirm)",
        message: "Net worth settings updated successfully.",
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  React.useEffect(() => {
    setSortedLineItems(
      props.group.lines
        .slice()
        .sort((a, b) => a.index - b.index)
        .map((line, index) => ({
          ...line,
          index,
        }))
    );
  }, [props.group.lines]);

  const prevIsSortable = React.useRef(props.isSortable);

  useDidUpdate(() => {
    if (prevIsSortable.current && !props.isSortable) {
      const orderedLines: string[] = sortedLineItems.map((line) => line.id);

      doReorderLines.mutate({
        widgetSettingsId: props.settingsId,
        groupId: props.group.id,
        orderedLineIds: orderedLines,
      } as INetWorthWidgetLineReorderRequest);
    }
    prevIsSortable.current = props.isSortable;
  }, [props.isSortable, sortedLineItems]);

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
      <LoadingOverlay visible={doReorderLines.isPending} />
      <Group gap="0.5rem">
        {props.isSortable && (
          <Flex ref={handleRef} style={{ alignSelf: "stretch" }}>
            <Button h="100%" px={0} w={30} radius="lg">
              <GripVertical size={25} />
            </Button>
          </Flex>
        )}
        <Stack flex="1 0 auto" gap="0.5rem">
          <Group justify="flex-end">
            <ActionIcon
              size="sm"
              loading={doCreateLine.isPending}
              onClick={async () =>
                await doCreateLine.mutateAsync({
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
                })
              );

              setSortedLineItems(updatedList);
            }}
          >
            <Stack id="lines-stack" gap="0.25rem">
              {sortedLineItems.map((line) => (
                <NetWorthLineItem
                  key={line.id}
                  container={document.getElementById("lines-stack") as Element}
                  line={line}
                  groupIndex={props.group.index}
                  lines={sortedLineItems}
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
