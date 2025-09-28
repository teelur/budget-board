import { Stack } from "@mantine/core";
import React from "react";
import AccountsContent from "./AccountsContent/AccountsContent";
import AccountsHeader from "./AccountsHeader/AccountsHeader";
import { useDisclosure } from "@mantine/hooks";

const Accounts = (): React.ReactNode => {
  const [isSortable, { toggle }] = useDisclosure(false);

  return (
    <Stack w="100%" maw={1400}>
      <AccountsHeader isSortable={isSortable} toggleSort={toggle} />
      <AccountsContent isSortable={isSortable} />
    </Stack>
  );
};

export default Accounts;
