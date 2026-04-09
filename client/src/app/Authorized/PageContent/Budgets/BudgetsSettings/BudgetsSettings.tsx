import { ActionIcon, Box, Flex, Group, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { ChevronLeftIcon, SendIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import {
  IUserSettings,
  IUserSettingsUpdateRequest,
} from "~/models/userSettings";

const BudgetsSettings = (): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();
  const navigate = useNavigate();

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

  const budgetWarningThresholdField = useField<number>({
    initialValue: userSettingsQuery.data?.budgetWarningThreshold ?? 80,
    validate: (value) =>
      value < 0 || value > 100
        ? t("budget_warning_threshold_invalid_message")
        : null,
  });

  const queryClient = useQueryClient();
  const doUpdateUserSettings = useMutation({
    mutationFn: async (updatedUserSettings: IUserSettingsUpdateRequest) =>
      await request({
        url: "/api/userSettings",
        method: "PUT",
        data: updatedUserSettings,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["userSettings"] });
    },
    onError: (error: any) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  React.useEffect(() => {
    if (userSettingsQuery.data) {
      budgetWarningThresholdField.setValue(
        userSettingsQuery.data.budgetWarningThreshold,
      );
    }
  }, [userSettingsQuery.data]);

  return (
    <Stack w="100%" p="0.5rem">
      <Group gap="xs">
        <ActionIcon variant="subtle" onClick={() => navigate("/budgets")}>
          <ChevronLeftIcon />
        </ActionIcon>
        <PrimaryText size="lg">{t("budget_settings")}</PrimaryText>
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

                doUpdateUserSettings.mutate({
                  budgetWarningThreshold:
                    budgetWarningThresholdField.getValue(),
                });
              }}
              loading={doUpdateUserSettings.isPending}
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
