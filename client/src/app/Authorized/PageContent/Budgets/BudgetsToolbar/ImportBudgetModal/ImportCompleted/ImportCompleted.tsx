import { Group, Stack } from "@mantine/core";
import { BodyText, Button } from "@teelur/budget-board-ui";
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
    <Stack align="center" gap="1rem" maw="100%">
      <Stack align="center" gap="0.25rem">
        <BodyText size="lg" fw={600}>
          {t("budget_import_success_title")}
        </BodyText>
        <BodyText size="sm" tone="secondary" ta="center">
          {t("budget_import_success_description", {
            count: importedCount,
            month: dayjs(targetMonth).format("MMMM YYYY"),
          })}
        </BodyText>
        {newCategoriesCount > 0 && (
          <BodyText size="xs" tone="secondary" ta="center">
            {t("budget_import_new_categories_created", {
              count: newCategoriesCount,
            })}
          </BodyText>
        )}
      </Stack>
      <Group w="100%">
        <Button
          variant="filled"
          color="primary"
          size="compact-sm"
          flex="1 1 auto"
          onClick={closeModal}
        >
          {t("close")}
        </Button>
      </Group>
    </Stack>
  );
};

export default ImportCompleted;
