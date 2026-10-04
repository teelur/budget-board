import { Stack } from "@mantine/core";
import { UseFieldReturnType } from "@mantine/form";
import React from "react";
import { useTranslation } from "react-i18next";
import { Checkbox, NumberInput } from "@teelur/budget-board-ui";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface SaveGoalOptionsProps {
  targetAmountField: UseFieldReturnType<number, "input", "controlled">;
  applyAccountAmountField: UseFieldReturnType<boolean, "input", "controlled">;
}

const SaveGoalOptions = (props: SaveGoalOptionsProps): React.ReactNode => {
  const { t } = useTranslation();
  const { thousandsSeparator, decimalSeparator, currencySymbol } = useLocale();
  const { decimalPlaces } = useUserSettings();

  return (
    <Stack gap={"0.5rem"}>
      <NumberInput
        label={t("target_amount")}
        placeholder={t("enter_target_amount")}
        prefix={currencySymbol}
        min={0}
        decimalScale={decimalPlaces}
        thousandSeparator={thousandsSeparator}
        decimalSeparator={decimalSeparator}
        {...props.targetAmountField.getInputProps()}
      />
      <Checkbox
        label={t("apply_existing_account_amount_to_goal")}
        checked={props.applyAccountAmountField.getValue()}
        onChange={(event) =>
          props.applyAccountAmountField.setValue(event.currentTarget.checked)
        }
      />
    </Stack>
  );
};

export default SaveGoalOptions;
