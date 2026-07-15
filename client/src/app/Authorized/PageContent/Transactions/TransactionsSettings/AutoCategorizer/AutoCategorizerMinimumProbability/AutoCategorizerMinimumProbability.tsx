import { LoadingOverlay, Stack } from "@mantine/core";
import React from "react";
import { IUserSettingsUpdateRequest } from "~/models/userSettings";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import { useField } from "@mantine/form";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { useUpdateUserSettingsMutation } from "~/hooks/mutations/userSettings/useUpdateUserSettingsMutation";

const AutoCategorizerMinimumProbability = (): React.ReactNode => {
  const { t } = useTranslation();
  const { autoCategorizerMinimumProbabilityPercentage } = useUserSettings();
  const updateUserSettingsMutation = useUpdateUserSettingsMutation();

  const minimumProbabilityField = useField<number>({
    initialValue: 0,
  });

  React.useEffect(() => {
    minimumProbabilityField.setValue(
      autoCategorizerMinimumProbabilityPercentage,
    );
  }, [autoCategorizerMinimumProbabilityPercentage]);

  return (
    <Stack gap="0.25rem">
      <LoadingOverlay visible={updateUserSettingsMutation.isPending} />
      <PrimaryText size="sm">
        {t("auto_categorizer_minimum_probability")}
      </PrimaryText>
      <DimmedText size="xs">
        {t("auto_categorizer_minimum_probability_description")}
      </DimmedText>
      <NumberInput
        {...minimumProbabilityField.getInputProps()}
        onBlur={() =>
          updateUserSettingsMutation.mutate({
            autoCategorizerMinimumProbabilityPercentage:
              minimumProbabilityField.getValue(),
          } as IUserSettingsUpdateRequest)
        }
        decimalScale={0}
        min={0}
        max={100}
        suffix="%"
        elevation={0}
      />
    </Stack>
  );
};

export default AutoCategorizerMinimumProbability;
