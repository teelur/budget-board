import { Button, Group, Stack } from "@mantine/core";
import { MoveLeftIcon } from "lucide-react";
import { useTranslation } from "react-i18next";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";

interface ImportCompletedProps {
  goBackToPreviousDialog: () => void;
  closeModal: () => void;
}

const ImportCompleted = (props: ImportCompletedProps) => {
  const { t } = useTranslation();

  return (
    <Stack justify="center" align="center" gap="0.5rem" w={600} maw="100%">
      <PrimaryText size="md" py="1rem">
        {t("import_completed_successfully")}
      </PrimaryText>
      <Group w="100%">
        <Button
          onClick={props.goBackToPreviousDialog}
          flex="1 1 auto"
          leftSection={<MoveLeftIcon size={16} />}
        >
          {t("back")}
        </Button>
        <Button onClick={props.closeModal} flex="1 1 auto">
          {t("close")}
        </Button>
      </Group>
    </Stack>
  );
};

export default ImportCompleted;
