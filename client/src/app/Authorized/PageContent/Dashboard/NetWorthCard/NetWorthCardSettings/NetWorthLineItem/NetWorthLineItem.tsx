import {
  ActionIcon,
  Button,
  Flex,
  Group,
  LoadingOverlay,
  Stack,
} from "@mantine/core";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { INetWorthWidgetLine } from "~/models/widgetSettings";
import NetWorthLineCategory from "./NetWorthLineCategory/NetWorthLineCategory";
import { GripVertical, PencilIcon, PlusIcon, TrashIcon } from "lucide-react";
import { useDisclosure } from "@mantine/hooks";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import { useField } from "@mantine/form";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  INetWorthWidgetCategoryCreateRequest,
  INetWorthWidgetLineUpdateRequest,
} from "~/models/netWorthWidgetConfiguration";
import { notifications } from "@mantine/notifications";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useSortable } from "@dnd-kit/react/sortable";
import { RestrictToVerticalAxis } from "@dnd-kit/abstract/modifiers";
import { RestrictToElement } from "@dnd-kit/dom/modifiers";
import { closestCorners } from "@dnd-kit/collision";

export interface INetWorthLineItemProps {
  container: Element;
  line: INetWorthWidgetLine;
  groupIndex: number;
  lines: INetWorthWidgetLine[];
  settingsId: string;
  isSortable: boolean;
}

const NetWorthLineItem = (props: INetWorthLineItemProps): React.ReactNode => {
  const [isEditing, { toggle }] = useDisclosure(false);
  const nameField = useField<string>({ initialValue: props.line.name });

  const { ref, handleRef } = useSortable({
    id: props.line.id,
    index: props.line.index,
    modifiers: [
      RestrictToElement.configure({ element: props.container }),
      RestrictToVerticalAxis,
    ],
    collisionDetector: closestCorners,
  });

  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doUpdateLine = useMutation({
    mutationFn: async (updatedLine: INetWorthWidgetLineUpdateRequest) =>
      await request({
        url: `/api/netWorthWidgetLine`,
        method: "PUT",
        data: updatedLine,
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

  const doDeleteLine = useMutation({
    mutationFn: async (id: string) =>
      await request({
        url: `/api/netWorthWidgetLine`,
        method: "DELETE",
        params: {
          lineId: id,
          widgetSettingsId: props.settingsId,
        },
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["widgetSettings"] });

      notifications.show({
        color: "var(--button-color-confirm)",
        message: "Net worth settings deleted successfully.",
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  const doCreateCategory = useMutation({
    mutationFn: async (categoryRequest: INetWorthWidgetCategoryCreateRequest) =>
      await request({
        url: `/api/netWorthWidgetCategory`,
        method: "POST",
        data: categoryRequest,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["widgetSettings"] });

      notifications.show({
        color: "var(--button-color-confirm)",
        message: "Net worth category created successfully.",
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  return (
    <Card ref={props.isSortable ? ref : undefined} elevation={1}>
      <LoadingOverlay visible={doUpdateLine.isPending} />
      <Group gap="0.5rem">
        {props.isSortable && (
          <Flex ref={handleRef} style={{ alignSelf: "stretch" }}>
            <Button h="100%" px={0} w={{ base: 25, xs: 30 }} radius="lg">
              <GripVertical size={25} />
            </Button>
          </Flex>
        )}
        <Stack flex="1 0 auto" gap="0.5rem">
          <Group justify="space-between">
            <Group gap="0.5rem">
              {isEditing ? (
                <TextInput
                  size="xs"
                  elevation={1}
                  {...nameField.getInputProps()}
                  onBlur={async () => {
                    if (nameField.getValue() !== props.line.name) {
                      await doUpdateLine.mutateAsync({
                        lineId: props.line.id,
                        name: nameField.getValue(),
                        group: props.groupIndex,
                        index: props.line.index,
                        widgetSettingsId: props.settingsId,
                      });
                    }
                  }}
                />
              ) : props.line.name.length > 0 ? (
                <PrimaryText size="sm">{props.line.name}</PrimaryText>
              ) : (
                <DimmedText size="sm">No Name</DimmedText>
              )}
              <ActionIcon
                size="sm"
                variant={isEditing ? "outline" : "transparent"}
                onClick={toggle}
              >
                <PencilIcon size={16} />
              </ActionIcon>
            </Group>

            <ActionIcon
              size="sm"
              loading={doCreateCategory.isPending}
              onClick={async () =>
                await doCreateCategory.mutateAsync({
                  lineId: props.line.id,
                  value: "",
                  type: "",
                  subtype: "",
                  widgetSettingsId: props.settingsId,
                } as INetWorthWidgetCategoryCreateRequest)
              }
            >
              <PlusIcon />
            </ActionIcon>
          </Group>
          <Stack gap="0.25rem">
            {props.line.categories.map((category) => (
              <NetWorthLineCategory
                key={category.id}
                category={category}
                lineId={props.line.id}
                currentLineName={props.line.name}
                lines={props.lines}
                settingsId={props.settingsId}
              />
            ))}
            {props.line.categories.length === 0 && (
              <Group justify="center">
                <DimmedText size="sm">No categories.</DimmedText>
              </Group>
            )}
          </Stack>
        </Stack>
        {isEditing && (
          <Flex style={{ alignSelf: "stretch" }}>
            <ActionIcon
              color="var(--button-color-destructive)"
              h="100%"
              size="md"
              loading={doDeleteLine.isPending}
              onClick={async () =>
                await doDeleteLine.mutateAsync(props.line.id)
              }
            >
              <TrashIcon size={16} />
            </ActionIcon>
          </Flex>
        )}
      </Group>
    </Card>
  );
};

export default NetWorthLineItem;
