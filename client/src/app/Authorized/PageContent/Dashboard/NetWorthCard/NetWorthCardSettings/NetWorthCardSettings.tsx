import { ActionIcon, Stack } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { SettingsIcon } from "lucide-react";
import Modal from "~/components/core/Modal/Modal";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import NetWorthGroupItem from "./NetWorthGroupItem/NetWorthGroupItem";
import React from "react";
import { useNetWorthSettings } from "~/providers/NetWorthSettingsProvider/NetWorthSettingsProvider";

const NetWorthCardSettings = (): React.ReactNode => {
  const [opened, { open, close }] = useDisclosure(false);

  const { lineGroups } = useNetWorthSettings();

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
          </Stack>
        </Stack>
      </Modal>
    </>
  );
};

export default NetWorthCardSettings;
