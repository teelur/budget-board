import { Stack } from "@mantine/core";
import React from "react";
import AccountsContent from "./AccountsContent/AccountsContent";
import AccountsHeader from "./AccountsHeader/AccountsHeader";

const Accounts = (): React.ReactNode => {
  return (
    <Stack w="100%" maw={1400}>
      <AccountsHeader />
      <AccountsContent />
    </Stack>
  );
};

export default Accounts;
