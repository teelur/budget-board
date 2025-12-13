import { ActionIcon, Button, Stack } from "@mantine/core";
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
  IWidgetSettingsResponse,
} from "~/models/widgetSettings";
import {
  isNetWorthWidgetType,
  parseNetWorthConfiguration,
} from "~/helpers/widgets";

const NetWorthCardSettings = (): React.ReactNode => {
  const [opened, { open, close }] = useDisclosure(false);
  const [isSortable, { toggle: toggleIsSortable }] = useDisclosure(false);

  const [sortedGroups, setSortedGroups] = React.useState<
    INetWorthWidgetGroup[]
  >([]);
  const [settingsId, setSettingsId] = React.useState<string>("");

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

  const doReorderGroups = useMutation({
    mutationFn: async (reorderRequest: INetWorthWidgetGroupReorderRequest) =>
      await request({
        url: `/api/netWorthWidgetGroup/reorder`,
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

  useDidUpdate(() => {
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
        title={<PrimaryText size="md">Net Worth Settings</PrimaryText>}
      >
        <Stack gap="0.5rem">
          <DimmedText size="sm">
            Configure the data that appears in the Net Worth widget.
          </DimmedText>
          <Button
            size="xs"
            bg={isSortable ? "var(--button-color-confirm)" : ""}
            onClick={toggleIsSortable}
          >
            {isSortable ? "Save Changes" : "Reorder"}
          </Button>
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
              <Stack id="groups-stack" gap="0.75rem">
                {sortedGroups.length > 0 ? (
                  sortedGroups.map((group) => (
                    <NetWorthGroupItem
                      key={group.id}
                      group={group}
                      isSortable={isSortable}
                      container={
                        document.getElementById("groups-stack") as Element
                      }
                      settingsId={settingsId}
                    />
                  ))
                ) : (
                  <DimmedText size="sm">No lines available.</DimmedText>
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
