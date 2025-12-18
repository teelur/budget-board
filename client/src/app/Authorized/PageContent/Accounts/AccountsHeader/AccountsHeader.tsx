import { Button, Group } from "@mantine/core";
import React from "react";
import CreateAccount from "./CreateAccount/CreateAccount";
import AccountsSettings from "./AccountsSettings/AccountsSettings";
import { useTranslation } from "react-i18next";

interface AccountsHeaderProps {
  isSortable: boolean;
  toggleSort: () => void;
}

const AccountsHeader = (props: AccountsHeaderProps): React.ReactNode => {
  const { t } = useTranslation();

  return (
    <Group w="100%" justify="flex-end" gap="0.5rem">
      <Button
        onClick={props.toggleSort}
        bg={props.isSortable ? "var(--button-color-confirm)" : undefined}
      >
        {props.isSortable ? t("save_changes") : t("reorder")}
      </Button>
      <CreateAccount />
      <AccountsSettings />
    </Group>
  );
};

export default AccountsHeader;
