import { Button, Group, Stack } from "@mantine/core";
import { MoveLeftIcon } from "lucide-react";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";

interface ImportCompletedProps {
  goBackToPreviousDialog: () => void;
  closeModal: () => void;
}

const ImportCompleted = (props: ImportCompletedProps) => {
  return (
    <Stack justify="center" align="center" gap="0.5rem" w={600} maw="100%">
      <PrimaryText size="md" py="1rem">
        Import Completed!
      </PrimaryText>
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
