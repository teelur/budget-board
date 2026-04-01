import { LoadingOverlay, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import {
  IUserSettings,
  IUserSettingsUpdateRequest,
} from "~/models/userSettings";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import Select from "~/components/core/Select/Select/Select";
import { useTranslation } from "react-i18next";

const ForceSyncLookbackPeriod = (): React.ReactNode => {
  interface IForceSyncOverrideOption {
    value: number;
    label: string;
  }

  const { t } = useTranslation();

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

  const forceSyncLookbackMonthsField = useField<number>({
    initialValue: userSettingsQuery.data?.forceSyncLookbackMonths || 0,
  });

  React.useEffect(() => {
    if (userSettingsQuery.data?.forceSyncLookbackMonths !== undefined) {
      forceSyncLookbackMonthsField.setValue(
        userSettingsQuery.data.forceSyncLookbackMonths
      );
    }
  }, [userSettingsQuery.data]);

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

  return (
    <Stack gap="0.25rem">
      <LoadingOverlay visible={doUpdateUserSettings.isPending} />
      <PrimaryText size="sm">{t("force_sync_lookback_period")}</PrimaryText>
      <DimmedText size="xs">
        {t("force_sync_lookback_period_description")}
      </DimmedText>
      <DimmedText size="xs">
        {t("force_sync_lookback_period_warning")}
      </DimmedText>
      <Select
        data={ForceSyncOverrideOptions.map((option) => ({
          value: option.value.toString(),
          label: option.label,
        }))}
        value={forceSyncLookbackMonthsField.getValue().toString()}
        onChange={(value) => {
          const intValue = parseInt(value || "0", 10);
          doUpdateUserSettings.mutate({
            forceSyncLookbackMonths: intValue,
          });
        }}
        elevation={1}
      />
    </Stack>
  );
};

export default ForceSyncLookbackPeriod;
