import { ActionIcon, Button, Stack } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { SettingsIcon } from "lucide-react";
import Modal from "~/components/core/Modal/Modal";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import NetWorthGroupItem from "./NetWorthGroupItem/NetWorthGroupItem";
import React from "react";
import { NetWorthSettingsContext } from "~/providers/NetWorthSettingsProvider/NetWorthSettingsProvider";

const NetWorthCardSettings = (): React.ReactNode => {
  const [opened, { open, close }] = useDisclosure(false);

  const netWorthSettings = React.useContext(NetWorthSettingsContext);

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
          {netWorthSettings.isDirty && (
            <Button
              size="xs"
              loading={netWorthSettings.isSavePending}
              onClick={async () => await netWorthSettings.saveChanges()}
            >
              Save Changes
            </Button>
          )}
          <Stack gap="1rem">
            {netWorthSettings.lineGroups.length > 0 ? (
              netWorthSettings.lineGroups.map((group, groupIndex) => {
                const lineIndexOffset = netWorthSettings.lineGroups
                  .slice(0, groupIndex)
                  .reduce((sum, g) => sum + g.lines.length, 0);
                return (
                  <NetWorthGroupItem
                    key={`net-worth-group-${group.groupId}`}
                    lines={group.lines}
                    lineIndexOffset={lineIndexOffset}
                    updateNetWorthLine={(updatedLine, lineIndex) => {
                      const allLines = netWorthSettings.lineGroups
                        .flatMap((g) => g.lines)
                        .slice();
                      allLines[lineIndex] = updatedLine;
                      // Update the context state
                      netWorthSettings.updateConfiguration({
                        lines: allLines,
                      });
                    }}
                  />
                );
              })
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
