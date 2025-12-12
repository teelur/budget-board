import { ActionIcon, Stack } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { PlusIcon, SettingsIcon } from "lucide-react";
import Modal from "~/components/core/Modal/Modal";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import NetWorthGroupItem from "./NetWorthGroupItem/NetWorthGroupItem";
import React from "react";
import { useNetWorthSettings } from "~/providers/NetWorthSettingsProvider/NetWorthSettingsProvider";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { INetWorthWidgetLineCreateRequest } from "~/models/netWorthWidgetConfiguration";
import { notifications } from "@mantine/notifications";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";

const NetWorthCardSettings = (): React.ReactNode => {
  const [opened, { open, close }] = useDisclosure(false);

  const { lineGroups, settingsId } = useNetWorthSettings();

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
        <Stack gap="1rem">
          <DimmedText size="sm">
            Configure the data that appears in the Net Worth widget.
          </DimmedText>
          <Stack gap="1rem">
            {lineGroups.length > 0 ? (
              lineGroups.map((group) => (
                <NetWorthGroupItem
                  key={`net-worth-group-${group.groupId}`}
                  lines={group.lines}
                />
              ))
            ) : (
              <DimmedText size="sm">No lines available.</DimmedText>
            )}
            <ActionIcon
              w="100%"
              loading={doCreateLine.isPending}
              onClick={() =>
                doCreateLine.mutate({
                  name: "",
                  group:
                    Math.max(...lineGroups.map((group) => group.groupId)) + 1,
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
