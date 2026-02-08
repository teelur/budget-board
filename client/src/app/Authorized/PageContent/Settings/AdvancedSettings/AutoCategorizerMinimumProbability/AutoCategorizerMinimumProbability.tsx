import { LoadingOverlay, Skeleton, Stack } from "@mantine/core";
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
import { useTranslation } from "react-i18next";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import { useField } from "@mantine/form";

const AutoCategorizerMinimumProbability = (): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();

  const minimumProbabilityField = useField<number>({
    initialValue: 0,
  });

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

  React.useEffect(() => {
    if (userSettingsQuery.data) {
      minimumProbabilityField.setValue(
        userSettingsQuery.data.autoCategorizerMinimumProbabilityPercentage,
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

  if (userSettingsQuery.isPending) {
    return <Skeleton height={75} radius="md" />;
  }

  return (
    <Stack gap="0.25rem">
      <LoadingOverlay visible={doUpdateUserSettings.isPending} />
      <PrimaryText size="sm">
        {t("auto_categorizer_minimum_probability")}
      </PrimaryText>
      <DimmedText size="xs">
        {t("auto_categorizer_minimum_probability_description")}
      </DimmedText>
      <NumberInput
        {...minimumProbabilityField.getInputProps()}
        onBlur={() =>
          doUpdateUserSettings.mutate({
            autoCategorizerMinimumProbabilityPercentage:
              minimumProbabilityField.getValue(),
          } as IUserSettingsUpdateRequest)
        }
        decimalScale={0}
        suffix="%"
        elevation={1}
      />
    </Stack>
  );
};

export default AutoCategorizerMinimumProbability;
