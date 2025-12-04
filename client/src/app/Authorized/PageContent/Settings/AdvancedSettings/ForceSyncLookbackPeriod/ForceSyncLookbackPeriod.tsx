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

const ForceSyncLookbackPeriod = (): React.ReactNode => {
  interface IForceSyncOverrideOption {
    value: number;
    label: string;
  }

  const ForceSyncOverrideOptions: IForceSyncOverrideOption[] = [
    { value: 0, label: "Auto" },
    { value: 1, label: "1 Month" },
    { value: 2, label: "2 Months" },
    { value: 3, label: "3 Months" },
    { value: 4, label: "4 Months" },
    { value: 5, label: "5 Months" },
    { value: 6, label: "6 Months" },
    { value: 7, label: "7 Months" },
    { value: 8, label: "8 Months" },
    { value: 9, label: "9 Months" },
    { value: 10, label: "10 Months" },
    { value: 11, label: "11 Months" },
    { value: 12, label: "12 Months" },
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
      <PrimaryText size="sm">Force Sync Lookback Period</PrimaryText>
      <DimmedText size="xs">
        This setting can be used to override the number of months to request
        from SimpleFIN during sync. Leave as "Auto" to let the system decide.
      </DimmedText>
      <DimmedText size="xs">
        Note: Leave this on auto for the best performance.
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
