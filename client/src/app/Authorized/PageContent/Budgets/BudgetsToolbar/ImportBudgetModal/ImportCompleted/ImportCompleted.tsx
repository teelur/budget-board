import { Group, Stack, Text, ThemeIcon } from "@mantine/core";
import { Button } from "@teelur/budget-board-ui";
import { CheckCircle2Icon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface ImportCompletedProps {
  importedCount: number;
  newCategoriesCount: number;
  targetMonth: Date;
  closeModal: () => void;
}

const ImportCompleted = ({
  importedCount,
  newCategoriesCount,
  targetMonth,
  closeModal,
}: ImportCompletedProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs } = useLocale();

  return (
    <Stack align="center" gap="1rem" py="2rem" w={500} maw="100%" mx="auto">
      <ThemeIcon color="green" size={60} radius="xl" variant="light">
        <CheckCircle2Icon size={36} />
      </ThemeIcon>

      <Stack align="center" gap="0.25rem">
        <Text size="lg" fw={600}>
          {t("budget_import_success_title")}
        </Text>
        <Text size="sm" c="dimmed" ta="center">
          {t("budget_import_success_description", {
            count: importedCount,
            month: dayjs(targetMonth).format("MMMM YYYY"),
          })}
        </Text>
        {newCategoriesCount > 0 && (
          <Text size="xs" c="dimmed" ta="center">
            {t("budget_import_new_categories_created", {
              count: newCategoriesCount,
            })}
          </Text>
        )}
      </Stack>

      <Group justify="center" mt="1rem">
        <Button variant="filled" color="primary" onClick={closeModal}>
          {t("done")}
        </Button>
      </Group>
    </Stack>
  );
};

export default ImportCompleted;
