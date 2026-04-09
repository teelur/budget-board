import { ActionIcon, Button, Group } from "@mantine/core";
import React from "react";
import CreateAsset from "./CreateAsset/CreateAsset";
import { useTranslation } from "react-i18next";
import { SettingsIcon } from "lucide-react";
import { useNavigate } from "react-router";

interface AssetsHeaderProps {
  isSortable: boolean;
  toggleSort: () => void;
}

const AssetsHeader = (props: AssetsHeaderProps): React.ReactNode => {
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
      <CreateAsset />
      <ActionIcon
        variant="subtle"
        size="input-sm"
        onClick={() => navigate("/assets/settings")}
      >
        <SettingsIcon />
      </ActionIcon>
    </Group>
  );
};

export default AssetsHeader;
