import { Stack } from "@mantine/core";
import React from "react";
import SimpleFinAccountsContent from "./SimpleFinAccountsContent/SimpleFinAccontsContent";

const ExternalAccountsContent = (): React.ReactNode => {
  return (
    <Stack p={0} gap="0.5rem">
      <SimpleFinAccountsContent />
    </Stack>
  );
};

export default ExternalAccountsContent;
