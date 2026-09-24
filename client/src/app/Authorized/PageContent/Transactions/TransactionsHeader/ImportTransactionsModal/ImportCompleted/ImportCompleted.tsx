import { Group, Stack } from "@mantine/core";
import { Button } from "@teelur/budget-board-ui";
import { MoveLeftIcon } from "lucide-react";
import { useTranslation } from "react-i18next";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";

interface ImportCompletedProps {
  goBackToPreviousDialog: () => void;
  closeModal: () => void;
  hasErrors?: boolean;
  isCancelled?: boolean;
  isFailed?: boolean;
}

const ImportCompleted = (props: ImportCompletedProps) => {
  const { t } = useTranslation();

  return (
    <Stack
      justify="center"
      align="center"
      gap="0.5rem"
      w={600}
      maw="100%"
      mx="auto"
    >
      <PrimaryText size="md" py="1rem">
        {t(
          props.isCancelled
            ? "import_stopped"
            : props.isFailed
              ? "import_failed"
              : props.hasErrors
                ? "import_completed_with_errors"
                : "import_completed_successfully",
        )}
      </PrimaryText>
      <Group w="100%">
        <Button
          variant="filled"
          color="primary"
          size="compact-sm"
          flex="1 1 auto"
          leftSection={<MoveLeftIcon size={16} />}
          onClick={props.goBackToPreviousDialog}
        >
          {t("back")}
        </Button>
        <Button
          variant="filled"
          color="primary"
          size="compact-sm"
          flex="1 1 auto"
          onClick={props.closeModal}
        >
          {t("close")}
        </Button>
      </Group>
    </Stack>
  );
};

export default ImportCompleted;
