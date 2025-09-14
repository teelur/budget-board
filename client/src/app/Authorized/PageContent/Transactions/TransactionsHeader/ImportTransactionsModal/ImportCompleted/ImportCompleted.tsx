import { Button, Group, Stack, Text } from "@mantine/core";
import { MoveLeftIcon } from "lucide-react";

interface ImportCompletedProps {
  goBackToPreviousDialog: () => void;
  closeModal: () => void;
}

const ImportCompleted = (props: ImportCompletedProps) => {
  return (
    <Stack justify="center" align="center" gap="0.5rem" w={600} maw="100%">
      <Text fw={600} size="md" py="1rem">
        Import Completed!
      </Text>
      <Group w="100%">
        <Button
          onClick={props.goBackToPreviousDialog}
          flex="1 1 auto"
          leftSection={<MoveLeftIcon size={16} />}
        >
          Back
        </Button>
        <Button onClick={props.closeModal} flex="1 1 auto">
          Close
        </Button>
      </Group>
    </Stack>
  );
};

export default ImportCompleted;
