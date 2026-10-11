import { Stack } from "@mantine/core";
import {
  BodyText,
  Button,
  DateInput,
  HeadingText,
} from "@teelur/budget-board-ui";
import { useField } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { NotificationType, showNotification } from "~/helpers/notifications";
import React from "react";
import Modal from "~/components/core/Modal/Modal";
import { useTranslation } from "react-i18next";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { useTrainAutomaticTransactionCategorizerMutation } from "~/hooks/mutations/automaticTransactionCategorizer/useTrainAutomaticTransactionCategorizerMutation";

const TrainAutoCategorizerModal = (): React.ReactNode => {
  const { t } = useTranslation();
  const {
    autoCategorizerLastTrained,
    autoCategorizerModelStartDate,
    autoCategorizerModelEndDate,
  } = useUserSettings();
  const trainAutomaticTransactionCategorizerMutation =
    useTrainAutomaticTransactionCategorizerMutation();

  const [opened, { open, close }] = useDisclosure(false);

  const startDateField = useField<Date | null>({
    initialValue: null,
  });
  const endDateField = useField<Date | null>({
    initialValue: null,
  });

  const onSubmit = () => {
    const startDate = startDateField.getValue();
    const endDate = endDateField.getValue();
    if (startDate != null && endDate != null && startDate > endDate) {
      showNotification({
        type: NotificationType.Error,
        message: t("train_auto_categorizer_dates_error"),
      });
      return;
    }

    trainAutomaticTransactionCategorizerMutation.mutate(
      {
        startDate: startDateField.getValue() ?? undefined,
        endDate: endDateField.getValue() ?? undefined,
      },
      {
        onSuccess: () => {
          startDateField.reset();
          endDateField.reset();
          close();
        },
      },
    );
  };

  return (
    <>
      <Stack gap={0}>
        <BodyText tone="primary" size="md" fw={500}>
          {t("train_auto_categorizer")}
        </BodyText>
        <BodyText tone="secondary" size="sm">
          {t("train_auto_categorizer_description")}
        </BodyText>
        <BodyText tone="primary" size="xs" fw={500}>
          {autoCategorizerLastTrained != null
            ? t("train_auto_categorizer_last_trained", {
                lastTrained: autoCategorizerLastTrained,
                trainDataStartDate: autoCategorizerModelStartDate,
                trainDataEndDate: autoCategorizerModelEndDate,
              })
            : t("train_auto_categorizer_not_trained")}
        </BodyText>
        <Button
          variant="filled"
          color="primary"
          size="compact-sm"
          onClick={open}
        >
          {t("train_auto_categorizer_button")}
        </Button>
      </Stack>
      <Modal
        opened={opened}
        onClose={close}
        title={
          <HeadingText tone="heading" level={4}>
            {t("train_auto_categorizer")}
          </HeadingText>
        }
      >
        <Stack gap="1rem">
          <Stack gap={0}>
            <BodyText tone="secondary" size="xs">
              {t("train_auto_categorizer_date_range_description")}
            </BodyText>
            <DateInput
              label={t("start_date")}
              placeholder={t("select_a_date")}
              {...startDateField.getInputProps()}
              clearable
            />
            <DateInput
              label={t("end_date")}
              placeholder={t("select_a_date")}
              {...endDateField.getInputProps()}
              clearable
            />
          </Stack>
          <Button
            variant="filled"
            color="primary"
            size="compact-sm"
            loading={trainAutomaticTransactionCategorizerMutation.isPending}
            onClick={onSubmit}
          >
            {t("submit")}
          </Button>
        </Stack>
      </Modal>
    </>
  );
};

export default TrainAutoCategorizerModal;
