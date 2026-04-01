import { ActionIcon, Button, Group, Stack } from "@mantine/core";
import { useDidUpdate, useDisclosure } from "@mantine/hooks";
import { PlusIcon, SettingsIcon } from "lucide-react";
import Modal from "~/components/core/Modal/Modal";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import NetWorthGroupItem from "./NetWorthGroupItem/NetWorthGroupItem";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  INetWorthWidgetGroupReorderRequest,
  INetWorthWidgetLineCreateRequest,
} from "~/models/netWorthWidgetConfiguration";
import { notifications } from "@mantine/notifications";
import { AxiosError, AxiosResponse } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { DragDropProvider } from "@dnd-kit/react";
import { move } from "@dnd-kit/helpers";
import {
  INetWorthWidgetGroup,
  INetWorthWidgetLine,
  IWidgetSettingsResponse,
} from "~/models/widgetSettings";
import {
  isNetWorthWidgetType,
  parseNetWorthConfiguration,
} from "~/helpers/widgets";
import { useTranslation } from "react-i18next";

const NetWorthCardSettings = (): React.ReactNode => {
  const [opened, { open, close }] = useDisclosure(false);
  const [isSortable, { toggle: toggleIsSortable }] = useDisclosure(false);

  const [sortedGroups, setSortedGroups] = React.useState<
    INetWorthWidgetGroup[]
  >([]);
  const [settingsId, setSettingsId] = React.useState<string>("");
  const [onReorderCompleted, setOnReorderCompleted] =
    React.useState<boolean>(false);

  const { t } = useTranslation();
  const { request } = useAuth();

  const widgetSettingsQuery = useQuery({
    queryKey: ["widgetSettings"],
    queryFn: async (): Promise<IWidgetSettingsResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/widgetSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IWidgetSettingsResponse[];
      }

      return [];
    },
  });

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
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

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

  const doResetConfig = useMutation({
    mutationFn: async () =>
      await request({
        url: `/api/widgetSettings/resetConfiguration`,
        method: "POST",
        params: {
          widgetGuid: settingsId,
        },
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["widgetSettings"] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  React.useEffect(() => {
    if (widgetSettingsQuery.data) {
      const foundWidget = widgetSettingsQuery.data.find((ws) =>
        isNetWorthWidgetType(ws.widgetType)
      );
      if (foundWidget) {
        const configuration = parseNetWorthConfiguration(
          foundWidget.configuration
        );

        setSettingsId(foundWidget.id);

        if (configuration) {
          setSortedGroups(
            configuration.groups
              ?.sort((a, b) => a.index - b.index)
              .map((line, index) => ({
                ...line,
                index,
              })) ?? []
          );
        }
      }
    }
  }, [widgetSettingsQuery.data]);

  const allLines = React.useMemo(() => {
    return sortedGroups.reduce<INetWorthWidgetLine[]>((acc, group) => {
      return [...acc, ...group.lines];
    }, []);
  }, [sortedGroups]);

  useDidUpdate(() => {
    if (!isSortable) {
      const orderedGroups: string[] = sortedGroups.map((group) => group.id);

      doReorderGroups.mutate({
        widgetSettingsId: settingsId,
        orderedGroupIds: orderedGroups,
      });
    }
  }, [isSortable]);

  const groupsStackRef = React.useRef<HTMLDivElement>(null);

  return (
    <>
      <ActionIcon
        variant="subtle"
        size="md"
        c="var(--base-color-text-dimmed)"
        onClick={open}
      >
        <SettingsIcon />
      </ActionIcon>
      <Modal
        size="40rem"
        opened={opened}
        onClose={close}
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
              loading={doResetConfig.isPending}
              onClick={() => doResetConfig.mutate()}
            >
              {t("reset_to_default")}
            </Button>
          </Group>
          <Stack gap="1rem">
            <DragDropProvider
              onDragEnd={(event) => {
                const updatedList = move(
                  sortedGroups,
                  event
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
                      settingsId={settingsId}
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
              loading={doCreateLine.isPending}
              onClick={() =>
                doCreateLine.mutate({
                  name: "",
                  group:
                    Math.max(...sortedGroups.map((group) => group.index)) + 1,
                  index: 0,
                  widgetSettingsId: settingsId,
                } as INetWorthWidgetLineCreateRequest)
              }
            >
              <PlusIcon />
            </ActionIcon>
          </Stack>
        </Stack>
      </Modal>
    </>
  );
};

export default NetWorthCardSettings;
