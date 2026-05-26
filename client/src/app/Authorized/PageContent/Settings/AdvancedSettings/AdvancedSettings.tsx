import { Stack } from "@mantine/core";
import ForceSyncLookbackPeriod from "./ForceSyncLookbackPeriod/ForceSyncLookbackPeriod";

const AdvancedSettings = () => {
  return (
    <Stack gap="0.5rem">
      <ForceSyncLookbackPeriod />
    </Stack>
  );
};

export default AdvancedSettings;
