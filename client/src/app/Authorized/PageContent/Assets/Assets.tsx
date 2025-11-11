import { Stack } from "@mantine/core";
import React from "react";
import { useDisclosure } from "@mantine/hooks";
import AssetsHeader from "./AssetsHeader/AssetsHeader";
import AssetsContent from "./AssetsContent/AssetsContent";

const Assets = (): React.ReactNode => {
  const [isSortable, { toggle }] = useDisclosure(false);

  return (
    <Stack w="100%" maw={1400}>
      <AssetsHeader isSortable={isSortable} toggleSort={toggle} />
      <AssetsContent isSortable={isSortable} />
    </Stack>
  );
};

export default Assets;
