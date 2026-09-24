import { Group } from "@mantine/core";
import { Button } from "@teelur/budget-board-ui";
import React from "react";
import CreateAccount from "./CreateAccount/CreateAccount";
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
        variant="filled"
        color="primary"
        size="xs"
        selected={props.isSortable}
        onClick={props.toggleSort}
      >
        {props.isSortable ? t("save_changes") : t("reorder")}
      </Button>
      <CreateAccount />
    </Group>
  );
};

export default AccountsHeader;
