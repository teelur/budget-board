import { LoadingOverlay, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import React from "react";
import { useTranslation } from "react-i18next";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { useUpdateUserSettingsMutation } from "~/hooks/mutations/userSettings/useUpdateUserSettingsMutation";
import { Select } from "@teelur/budget-board-ui";

const ForceSyncLookbackPeriod = (): React.ReactNode => {
  interface IForceSyncOverrideOption {
    value: number;
    label: string;
  }

  const { t } = useTranslation();
  const { forceSyncLookbackMonths } = useUserSettings();
  const updateUserSettingsMutation = useUpdateUserSettingsMutation();

  const ForceSyncOverrideOptions: IForceSyncOverrideOption[] = [
    { value: 0, label: t("auto") },
    { value: 1, label: t("1_month") },
    { value: 2, label: t("2_months") },
    { value: 3, label: t("3_months") },
    { value: 4, label: t("4_months") },
    { value: 5, label: t("5_months") },
    { value: 6, label: t("6_months") },
    { value: 7, label: t("7_months") },
    { value: 8, label: t("8_months") },
    { value: 9, label: t("9_months") },
    { value: 10, label: t("10_months") },
    { value: 11, label: t("11_months") },
    { value: 12, label: t("12_months") },
  ];

  const forceSyncLookbackMonthsField = useField<number>({
    initialValue: forceSyncLookbackMonths,
  });

  React.useEffect(() => {
    forceSyncLookbackMonthsField.setValue(forceSyncLookbackMonths);
  }, [forceSyncLookbackMonths]);

  return (
    <Stack gap="0.25rem">
      <LoadingOverlay visible={updateUserSettingsMutation.isPending} />
      <Select
        data={ForceSyncOverrideOptions.map((option) => ({
          value: option.value.toString(),
          label: option.label,
        }))}
        value={forceSyncLookbackMonthsField.getValue().toString()}
        onChange={(value) => {
          const intValue = parseInt(value || "0", 10);
          updateUserSettingsMutation.mutate({
            forceSyncLookbackMonths: intValue,
          });
        }}
        label={t("force_sync_lookback_period")}
        description={t("force_sync_lookback_period_description")}
      />
    </Stack>
  );
};

export default ForceSyncLookbackPeriod;
