import { ActionIcon, Button, Group } from "@mantine/core";
import { PlusIcon, SettingsIcon } from "lucide-react";
import React from "react";

const AccountsHeader = (): React.ReactNode => {
  return (
    <Group w="100%" justify="flex-end" gap="0.5rem">
      <Button>Reorder</Button>
      <ActionIcon size="input-sm">
        <PlusIcon />
      </ActionIcon>
      <ActionIcon variant="subtle" size="input-sm">
        <SettingsIcon />
      </ActionIcon>
    </Group>
  );
};

export default AccountsHeader;
