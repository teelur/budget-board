import { LoadingOverlay, Stack } from "@mantine/core";
import { BodyText, NumberInput } from "@teelur/budget-board-ui";
import React from "react";
import { IUserSettingsUpdateRequest } from "~/models/userSettings";
import { useTranslation } from "react-i18next";
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
    <Stack gap={0}>
      <LoadingOverlay visible={updateUserSettingsMutation.isPending} />
      <BodyText tone="primary" size="md" fw={500}>
        {t("auto_categorizer_minimum_probability")}
      </BodyText>
      <BodyText tone="secondary" size="sm">
        {t("auto_categorizer_minimum_probability_description")}
      </BodyText>
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
      />
    </Stack>
  );
};

export default AutoCategorizerMinimumProbability;
