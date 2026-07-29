import { ActionIcon, Button, Group, Stack } from "@mantine/core";
import { useDidUpdate, useDisclosure } from "@mantine/hooks";
import { PlusIcon } from "lucide-react";
import Modal from "~/components/core/Modal/Modal";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import NetWorthGroupItem from "./NetWorthGroupItem/NetWorthGroupItem";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useMutation } from "@tanstack/react-query";
import {
  INetWorthWidgetGroupReorderRequest,
  INetWorthWidgetLineCreateRequest,
} from "~/models/netWorthWidgetConfiguration";
import { notifications } from "@mantine/notifications";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { DragDropProvider } from "@dnd-kit/react";
import { move } from "@dnd-kit/helpers";
import {
  INetWorthWidgetGroup,
  INetWorthWidgetLine,
  IWidgetSettingsResponse,
} from "~/models/widgetSettings";
import { parseNetWorthConfiguration } from "~/helpers/widgets";
import { useTranslation } from "react-i18next";
import { useUpdateWidgetSettingsMutation } from "~/hooks/mutations/widgetSettings/useUpdateWidgetSettingsMutation";
import { useCreateNetWorthWidgetLineMutation } from "~/hooks/mutations/netWorthWidgetLine/useCreateNetWorthWidgetLineMutation";

interface NetWorthCardSettingsProps {
  widget: IWidgetSettingsResponse;
  opened: boolean;
  onClose: () => void;
}

const NetWorthCardSettings = ({
  widget,
  opened,
  onClose,
}: NetWorthCardSettingsProps): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();
  const updateWidgetSettingsMutation = useUpdateWidgetSettingsMutation();
  const createNetWorthWidgetLineMutation =
    useCreateNetWorthWidgetLineMutation();

  const [isSortable, { toggle: toggleIsSortable }] = useDisclosure(false);

  const [sortedGroups, setSortedGroups] = React.useState<
    INetWorthWidgetGroup[]
  >([]);
  const [onReorderCompleted, setOnReorderCompleted] =
    React.useState<boolean>(false);

  const doReorderGroups = useMutation({
    mutationFn: async (reorderRequest: INetWorthWidgetGroupReorderRequest) =>
      await request({
        url: `/api/netWorthWidgetGroup/reorder`,
        method: "POST",
        data: reorderRequest,
      }),
    onSuccess: () => {
      setOnReorderCompleted((prev) => !prev);
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  React.useEffect(() => {
    const configuration = parseNetWorthConfiguration(widget.configuration);
    if (configuration) {
      setSortedGroups(
        configuration.groups
          ?.sort((a, b) => a.index - b.index)
          .map((line, index) => ({
            ...line,
            index,
          })) ?? [],
      );
    }
  }, [widget]);

  const allLines = React.useMemo(() => {
    return sortedGroups.reduce<INetWorthWidgetLine[]>((acc, group) => {
      return [...acc, ...group.lines];
    }, []);
  }, [sortedGroups]);

  useDidUpdate(() => {
    if (!isSortable) {
      const orderedGroups: string[] = sortedGroups.map((group) => group.id);

      doReorderGroups.mutate({
        widgetSettingsId: widget.id,
        orderedGroupIds: orderedGroups,
      });
    }
  }, [isSortable]);

  const groupsStackRef = React.useRef<HTMLDivElement>(null);

  return (
    <Modal
      size="40rem"
      opened={opened}
      onClose={onClose}
      title={<PrimaryText size="md">{t("net_worth_settings")}</PrimaryText>}
    >
      <Stack gap="0.5rem">
        <DimmedText size="sm">
          {t("net_worth_settings_widget_message")}
        </DimmedText>
        <Group w="100%">
          <Button
            flex="1 0 auto"
            size="xs"
            bg={isSortable ? "var(--button-color-confirm)" : ""}
            onClick={toggleIsSortable}
          >
            {isSortable ? t("save_changes") : t("reorder")}
          </Button>
          <Button
            size="xs"
            loading={updateWidgetSettingsMutation.isPending}
            onClick={() =>
              updateWidgetSettingsMutation.mutate([
                {
                  id: widget.id,
                  configuration: null,
                },
              ])
            }
          >
            {t("reset_to_default")}
          </Button>
        </Group>
        <Stack gap="1rem">
          <DragDropProvider
            onDragEnd={(event) => {
              const updatedList = move(
                sortedGroups,
                event,
              ).map<INetWorthWidgetGroup>((group, index) => ({
                ...group,
                index,
              }));

              setSortedGroups(updatedList);
            }}
          >
            <Stack ref={groupsStackRef} gap="0.75rem">
              {sortedGroups.length > 0 ? (
                sortedGroups.map((group) => (
                  <NetWorthGroupItem
                    key={group.id}
                    group={group}
                    isSortable={isSortable}
                    container={groupsStackRef.current as Element}
                    settingsId={widget.id}
                    onReorder={onReorderCompleted}
                    allLines={allLines}
                  />
                ))
              ) : (
                <DimmedText size="sm">{t("no_lines_found")}</DimmedText>
              )}
            </Stack>
          </DragDropProvider>
          <ActionIcon
            w="100%"
            loading={createNetWorthWidgetLineMutation.isPending}
            onClick={() =>
              createNetWorthWidgetLineMutation.mutate({
                name: "",
                group:
                  Math.max(...sortedGroups.map((group) => group.index)) + 1,
                index: 0,
                widgetSettingsId: widget.id,
              } as INetWorthWidgetLineCreateRequest)
            }
          >
            <PlusIcon />
          </ActionIcon>
        </Stack>
      </Stack>
    </Modal>
  );
};

export default NetWorthCardSettings;
