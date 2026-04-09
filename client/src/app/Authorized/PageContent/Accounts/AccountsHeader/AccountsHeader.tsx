import { ActionIcon, Button, Group } from "@mantine/core";
import React from "react";
import CreateAccount from "./CreateAccount/CreateAccount";
import { useTranslation } from "react-i18next";
import { SettingsIcon } from "lucide-react";
import { useNavigate } from "react-router";

interface AccountsHeaderProps {
  isSortable: boolean;
  toggleSort: () => void;
}

const AccountsHeader = (props: AccountsHeaderProps): React.ReactNode => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  return (
    <Group w="100%" justify="flex-end" gap="0.5rem">
      <Button
        onClick={props.toggleSort}
        bg={props.isSortable ? "var(--button-color-confirm)" : undefined}
      >
        {props.isSortable ? t("save_changes") : t("reorder")}
      </Button>
      <CreateAccount />
      <ActionIcon
        variant="subtle"
        size="input-sm"
        onClick={() => navigate("/accounts/settings")}
      >
        <SettingsIcon />
      </ActionIcon>
    </Group>
  );
};

export default AccountsHeader;
