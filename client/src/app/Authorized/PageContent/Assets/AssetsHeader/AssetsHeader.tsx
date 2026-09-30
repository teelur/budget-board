import { Group } from "@mantine/core";
import { Button } from "@teelur/budget-board-ui";
import React from "react";
import CreateAsset from "./CreateAsset/CreateAsset";
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
        variant="filled"
        color="primary"
        size="xs"
        selected={props.isSortable}
        onClick={props.toggleSort}
      >
        {props.isSortable ? t("save_changes") : t("reorder")}
      </Button>
      <CreateAsset />
    </Group>
  );
};

export default AssetsHeader;
