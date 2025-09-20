import { ActionIcon, Button, Group } from "@mantine/core";
import { SettingsIcon } from "lucide-react";
import React from "react";
import CreateAccount from "./CreateAccount/CreateAccount";

interface AccountsHeaderProps {
  isSortable: boolean;
  toggleSort: () => void;
}

const AccountsHeader = (props: AccountsHeaderProps): React.ReactNode => {
  return (
    <Group w="100%" justify="flex-end" gap="0.5rem">
      <Button
        onClick={props.toggleSort}
        variant={props.isSortable ? "outline" : "primary"}
      >
        {props.isSortable ? "Save" : "Reorder"}
      </Button>
      <CreateAccount />
      <ActionIcon variant="subtle" size="input-sm">
        <SettingsIcon />
      </ActionIcon>
    </Group>
  );
};

export default AccountsHeader;
