import { Button, Group } from "@mantine/core";
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
        bg={props.isSortable ? "green" : undefined}
      >
        {props.isSortable ? "Save" : "Reorder"}
      </Button>
      <CreateAccount />
    </Group>
  );
};

export default AccountsHeader;
