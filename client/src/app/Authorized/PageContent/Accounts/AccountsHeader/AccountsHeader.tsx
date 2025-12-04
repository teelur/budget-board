import { Button, Group } from "@mantine/core";
import React from "react";
import CreateAccount from "./CreateAccount/CreateAccount";
import AccountsSettings from "./AccountsSettings/AccountsSettings";

interface AccountsHeaderProps {
  isSortable: boolean;
  toggleSort: () => void;
}

const AccountsHeader = (props: AccountsHeaderProps): React.ReactNode => {
  return (
    <Group w="100%" justify="flex-end" gap="0.5rem">
      <Button
        onClick={props.toggleSort}
        bg={props.isSortable ? "var(--button-color-confirm)" : undefined}
      >
        {props.isSortable ? "Save" : "Reorder"}
      </Button>
      <CreateAccount />
      <AccountsSettings />
    </Group>
  );
};

export default AccountsHeader;
