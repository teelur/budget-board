import { Stack } from "@mantine/core";
import React from "react";
import SimpleFinAccountsContent from "./SimpleFinAccountsContent/SimpleFinAccontsContent";
import LunchFlowAccountsContent from "./LunchFlowAccountsContent/LunchFlowAccountsContent";
import ToshlAccountsContent from "./ToshlAccountsContent/ToshlAccountsContent";

const ExternalAccountsContent = (): React.ReactNode => {
  return (
    <Stack p={0} gap="0.5rem">
      <SimpleFinAccountsContent />
      <LunchFlowAccountsContent />
      <ToshlAccountsContent />
    </Stack>
  );
};

export default ExternalAccountsContent;
