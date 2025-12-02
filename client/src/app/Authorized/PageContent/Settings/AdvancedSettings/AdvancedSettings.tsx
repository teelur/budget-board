import { Stack } from "@mantine/core";
import ForceSyncLookbackPeriod from "./ForceSyncLookbackPeriod/ForceSyncLookbackPeriod";
import DisableBuiltInTransactionCategories from "./DisableBuiltInTransactionCategories/DisableBuiltInTransactionCategories";
import Card from "~/components/Card/Card";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";

const AdvancedSettings = () => {
  return (
    <Card elevation={1}>
      <Stack gap="0.5rem">
        <PrimaryText size="lg">Advanced Settings</PrimaryText>
        <DisableBuiltInTransactionCategories />
        <ForceSyncLookbackPeriod />
      </Stack>
    </Card>
  );
};

export default AdvancedSettings;
