import { ActionIcon, Flex, Group } from "@mantine/core";
import { useField } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { SendIcon, SettingsIcon } from "lucide-react";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import {
  IUserSettings,
  IUserSettingsUpdateRequest,
} from "~/models/userSettings";
import Modal from "~/components/core/Modal/Modal";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";

const BudgetSettings = (): React.ReactNode => {
  const [settingsOpen, { open, close }] = useDisclosure(false);

  const { t } = useTranslation();
  const { request } = useAuth();

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
        userSettingsQuery.data.budgetWarningThreshold
      );
    }
  }, [userSettingsQuery.data]);
  return (
    <>
      <ActionIcon variant="subtle" size="input-sm" onClick={open}>
        <SettingsIcon />
      </ActionIcon>
      <Modal
        opened={settingsOpen}
        onClose={close}
        title={<PrimaryText>{t("budget_settings")}</PrimaryText>}
      >
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
      </Modal>
    </>
  );
};

export default BudgetSettings;
