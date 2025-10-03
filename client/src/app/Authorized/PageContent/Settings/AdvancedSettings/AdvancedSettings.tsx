import { Card, Stack, Text } from "@mantine/core";
import ForceSyncLookbackPeriod from "./ForceSyncLookbackPeriod/ForceSyncLookbackPeriod";

const AdvancedSettings = () => {
  return (
    <Card p="0.5rem" withBorder radius="md" shadow="sm">
      <Stack gap="0.5rem">
        <Text size="lg" fw={700}>
          Advanced Settings
        </Text>
        <ForceSyncLookbackPeriod />
      </Stack>
    </Card>
  );
};

export default AdvancedSettings;
