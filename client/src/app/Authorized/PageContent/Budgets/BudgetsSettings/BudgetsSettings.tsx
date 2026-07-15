import { ActionIcon, Box, Flex, Group, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { ChevronLeftIcon, SendIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { useUpdateUserSettingsMutation } from "~/hooks/mutations/userSettings/useUpdateUserSettingsMutation";

const BudgetsSettings = (): React.ReactNode => {
  const { t } = useTranslation();
  const { budgetWarningThreshold } = useUserSettings();
  const updateUserSettingsMutation = useUpdateUserSettingsMutation();
  const navigate = useNavigate();

  const budgetWarningThresholdField = useField<number>({
    initialValue: budgetWarningThreshold,
    validate: (value) =>
      value < 0 || value > 100
        ? t("budget_warning_threshold_invalid_message")
        : null,
  });

  React.useEffect(() => {
    budgetWarningThresholdField.setValue(budgetWarningThreshold);
  }, [budgetWarningThreshold]);

  return (
    <Stack w="100%" p="0.5rem">
      <Group gap="xs">
        <ActionIcon variant="subtle" onClick={() => navigate("/budgets")}>
          <ChevronLeftIcon />
        </ActionIcon>
        <PrimaryHeading order={4}>{t("budget_settings")}</PrimaryHeading>
      </Group>
      <Box maw={800} mx="auto" w="100%">
        <Group gap="0.5rem" wrap="nowrap">
          <NumberInput
            flex="1 1 auto"
            label={
              <PrimaryText size="sm">
                {t("budget_warning_threshold")}
              </PrimaryText>
            }
            description={
              <DimmedText size="xs">
                {t("budget_warning_threshold_description")}
              </DimmedText>
            }
            min={0}
            max={100}
            suffix="%"
            {...budgetWarningThresholdField.getInputProps()}
          />
          <Flex style={{ alignSelf: "stretch" }} p={0}>
            <ActionIcon
              h="100%"
              size="md"
              onClick={() => {
                if (budgetWarningThresholdField.error) {
                  notifications.show({
                    color: "var(--button-color-destructive)",
                    message: budgetWarningThresholdField.error,
                  });
                  return;
                }

                updateUserSettingsMutation.mutate({
                  budgetWarningThreshold:
                    budgetWarningThresholdField.getValue(),
                });
              }}
              loading={updateUserSettingsMutation.isPending}
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
