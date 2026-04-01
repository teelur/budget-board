import { Stack } from "@mantine/core";
import React from "react";
import ExternalAccountsContent from "./ExternalAccountsContent/ExternalAccountsContent";

const ExternalAccounts = (): React.ReactNode => {
  return (
    <Stack w="100%" maw={1400} h="100%" flex="1" justify="space-between">
      <ExternalAccountsContent />
    </Stack>
  );
};

export default ExternalAccounts;
