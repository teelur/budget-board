import { Button, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import React from "react";
import Modal from "~/components/core/Modal/Modal";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
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
      notifications.show({
        color: "var(--button-color-destructive)",
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
      <PrimaryText size="sm">{t("train_auto_categorizer")}</PrimaryText>
      <DimmedText size="xs">
        {t("train_auto_categorizer_description")}
      </DimmedText>
      <DimmedText size="xs">
        {autoCategorizerLastTrained != null
          ? t("train_auto_categorizer_last_trained", {
              lastTrained: autoCategorizerLastTrained,
              trainDataStartDate: autoCategorizerModelStartDate,
              trainDataEndDate: autoCategorizerModelEndDate,
            })
          : t("train_auto_categorizer_not_trained")}
      </DimmedText>
      <Button size="xs" onClick={open}>
        {t("train_auto_categorizer_button")}
      </Button>
      <Modal
        opened={opened}
        onClose={close}
        title={<PrimaryText>{t("train_auto_categorizer")}</PrimaryText>}
      >
        <Stack gap="0.25rem">
          <DimmedText size="xs">
            {t("train_auto_categorizer_date_range_description")}
          </DimmedText>
          <DateInput
            label={<PrimaryText size="sm">{t("start_date")}</PrimaryText>}
            placeholder={t("select_a_date")}
            {...startDateField.getInputProps()}
            elevation={0}
            clearable
          />
          <DateInput
            label={<PrimaryText size="sm">{t("end_date")}</PrimaryText>}
            placeholder={t("select_a_date")}
            {...endDateField.getInputProps()}
            elevation={0}
            clearable
          />
          <Button
            mt="0.25rem"
            onClick={onSubmit}
            loading={trainAutomaticTransactionCategorizerMutation.isPending}
          >
            {t("submit")}
          </Button>
        </Stack>
      </Modal>
    </>
  );
};

export default TrainAutoCategorizerModal;
