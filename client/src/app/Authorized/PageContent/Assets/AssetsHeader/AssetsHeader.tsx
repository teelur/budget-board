import { Button, Group } from "@mantine/core";
import React from "react";
import CreateAsset from "./CreateAsset/CreateAsset";
import AssetsSettings from "./AssetsSettings/AssetsSettings";
import { useTranslation } from "react-i18next";

interface AssetsHeaderProps {
  isSortable: boolean;
  toggleSort: () => void;
}

const AssetsHeader = (props: AssetsHeaderProps): React.ReactNode => {
  const { t } = useTranslation();
  return (
    <Group w="100%" justify="flex-end" gap="0.5rem">
      <Button
        onClick={props.toggleSort}
        bg={props.isSortable ? "var(--button-color-confirm)" : undefined}
      >
        {props.isSortable ? t("save_changes") : t("reorder")}
      </Button>
      <CreateAsset />
      <AssetsSettings />
    </Group>
  );
};

export default AssetsHeader;
