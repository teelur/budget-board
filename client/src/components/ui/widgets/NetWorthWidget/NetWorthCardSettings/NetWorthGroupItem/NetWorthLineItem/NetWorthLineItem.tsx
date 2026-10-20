import { Flex, Group, Stack } from "@mantine/core";
import {
  ActionIcon,
  BodyText,
  Button,
  Card,
  LoadingOverlay,
  TextInput,
} from "@teelur/budget-board-ui";
import { INetWorthWidgetLine } from "~/models/widgetSettings";
import NetWorthLineCategory from "./NetWorthLineCategory/NetWorthLineCategory";
import { GripVertical, PencilIcon, PlusIcon, TrashIcon } from "lucide-react";
import { useDisclosure } from "@mantine/hooks";
import { useField } from "@mantine/form";
import { INetWorthWidgetCategoryCreateRequest } from "~/models/netWorthWidgetConfiguration";
import { useSortable } from "@dnd-kit/react/sortable";
import { RestrictToVerticalAxis } from "@dnd-kit/abstract/modifiers";
import { RestrictToElement } from "@dnd-kit/dom/modifiers";
import { closestCorners } from "@dnd-kit/collision";
import React from "react";
import { useTranslation } from "react-i18next";
import { useDeleteNetWorthWidgetLineMutation } from "~/hooks/mutations/netWorthWidgetLine/useDeleteNetWorthWidgetLineMutation";
import { useUpdateNetWorthWidgetLineMutation } from "~/hooks/mutations/netWorthWidgetLine/useUpdateNetWorthWidgetLineMutation";
import { useCreateNetWorthWidgetCategoryMutation } from "~/hooks/mutations/netWorthWidgetCategory/useCreateNetWorthWidgetCategoryMutation";

export interface INetWorthLineItemProps {
  container: Element;
  line: INetWorthWidgetLine;
  groupIndex: number;
  lines: INetWorthWidgetLine[];
  settingsId: string;
  isSortable: boolean;
}

const NetWorthLineItem = (props: INetWorthLineItemProps): React.ReactNode => {
  const { t } = useTranslation();
  const updateNetWorthWidgetLineMutation =
    useUpdateNetWorthWidgetLineMutation();
  const deleteNetWorthWidgetLineMutation =
    useDeleteNetWorthWidgetLineMutation();
  const createNetWorthWidgetCategoryMutation =
    useCreateNetWorthWidgetCategoryMutation();

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

  return (
    <Card ref={props.isSortable ? ref : undefined} surface="sunken">
      <LoadingOverlay visible={updateNetWorthWidgetLineMutation.isPending} />
      <Group gap="0.5rem">
        {props.isSortable && (
          <Flex ref={handleRef} style={{ alignSelf: "stretch" }}>
            <Button
              variant="filled"
              color="secondary"
              h="100%"
              px={0}
              w={{ base: 25, xs: 30 }}
            >
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
                  {...nameField.getInputProps()}
                  onBlur={async () => {
                    if (nameField.getValue() !== props.line.name) {
                      await updateNetWorthWidgetLineMutation.mutateAsync({
                        lineId: props.line.id,
                        name: nameField.getValue(),
                        widgetSettingsId: props.settingsId,
                      });
                    }
                  }}
                />
              ) : props.line.name.length > 0 ? (
                <BodyText tone="primary" size="sm" fw={500}>
                  {props.line.name}
                </BodyText>
              ) : (
                <BodyText tone="muted" size="sm" fw={500}>
                  {t("no_name")}
                </BodyText>
              )}
              <ActionIcon
                variant="ghost"
                color="primary"
                size="compact-xs"
                selected={isEditing}
                onClick={toggle}
              >
                <PencilIcon size={16} />
              </ActionIcon>
            </Group>

            <ActionIcon
              variant="filled"
              color="primary"
              size="compact-xs"
              loading={createNetWorthWidgetCategoryMutation.isPending}
              onClick={async () =>
                await createNetWorthWidgetCategoryMutation.mutateAsync({
                  lineId: props.line.id,
                  value: "",
                  type: "",
                  subtype: "",
                  widgetSettingsId: props.settingsId,
                } as INetWorthWidgetCategoryCreateRequest)
              }
            >
              <PlusIcon size={20} />
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
              <BodyText tone="muted" size="xs" ta="center">
                {t("no_categories")}
              </BodyText>
            )}
          </Stack>
        </Stack>
        {isEditing && (
          <Flex style={{ alignSelf: "stretch" }}>
            <ActionIcon
              variant="filled"
              color="error"
              size="compact-sm"
              loading={deleteNetWorthWidgetLineMutation.isPending}
              h="100%"
              onClick={async () =>
                await deleteNetWorthWidgetLineMutation.mutateAsync({
                  lineId: props.line.id,
                  widgetSettingsId: props.settingsId,
                })
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
