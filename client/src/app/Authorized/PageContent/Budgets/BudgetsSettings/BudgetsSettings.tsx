import { Box, Flex, Group, Stack } from "@mantine/core";
import { ActionIcon, NumberInput } from "@teelur/budget-board-ui";
import { useField } from "@mantine/form";
import { NotificationType, showNotification } from "~/helpers/notifications";
import { SendIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import SettingsHeading from "~/components/SettingsHeading/SettingsHeading";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { useUpdateUserSettingsMutation } from "~/hooks/mutations/userSettings/useUpdateUserSettingsMutation";

const BudgetsSettings = (): React.ReactNode => {
  const { t } = useTranslation();
  const { budgetWarningThreshold } = useUserSettings();
  const updateUserSettingsMutation = useUpdateUserSettingsMutation();

  const budgetWarningThresholdField = useField<number>({
    initialValue: budgetWarningThreshold,
    validate: (value) =>
      value < 0 || value > 100
        ? t("budget_warning_threshold_invalid_message")
        : null,
  });

  const navItems = [{ path: "settings", label: t("settings") }];

  const activeItem = navItems.find((item) =>
    location.pathname.endsWith(item.path),
  );

  React.useEffect(() => {
    budgetWarningThresholdField.setValue(budgetWarningThreshold);
  }, [budgetWarningThreshold]);

  return (
    <Stack w="100%" p="0.5rem">
      <SettingsHeading
        title={t("budgets")}
        backTo="/budgets"
        activeItem={activeItem?.label}
      />
      <Box
        w={{ base: "100%", sm: "auto" }}
        maw={800}
        style={{ flex: 1, minWidth: 0 }}
      >
        <Group gap="0.5rem" wrap="nowrap">
          <NumberInput
            flex="1 1 auto"
            label={t("budget_warning_threshold")}
            description={t("budget_warning_threshold_description")}
            min={0}
            max={100}
            suffix="%"
            {...budgetWarningThresholdField.getInputProps()}
          />
          <Flex style={{ alignSelf: "stretch" }} p={0}>
            <ActionIcon
              variant="filled"
              color="primary"
              size="compact-xs"
              loading={updateUserSettingsMutation.isPending}
              h="100%"
              onClick={() => {
                if (budgetWarningThresholdField.error) {
                  showNotification({
                    type: NotificationType.Error,
                    message: budgetWarningThresholdField.error,
                  });
                  return;
                }

                updateUserSettingsMutation.mutate({
                  budgetWarningThreshold:
                    budgetWarningThresholdField.getValue(),
                });
              }}
            >
              <SendIcon size={20} />
            </ActionIcon>
          </Flex>
        </Group>
      </Box>
    </Stack>
  );
};

export default BudgetsSettings;
