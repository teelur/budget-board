import { Button, Group } from "@mantine/core";
import React from "react";
import CreateAsset from "./CreateAsset/CreateAsset";

interface AssetsHeaderProps {
  isSortable: boolean;
  toggleSort: () => void;
}

const AssetsHeader = (props: AssetsHeaderProps): React.ReactNode => {
  return (
    <Group w="100%" justify="flex-end" gap="0.5rem">
      <Button
        onClick={props.toggleSort}
        bg={props.isSortable ? "green" : undefined}
      >
        {props.isSortable ? "Save" : "Reorder"}
      </Button>
      <CreateAsset />
    </Group>
  );
};

export default AssetsHeader;
