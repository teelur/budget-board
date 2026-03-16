import { Stack, Switch } from "@mantine/core";
import { UseFieldReturnType } from "@mantine/form";
import React from "react";
import { useTranslation } from "react-i18next";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface SaveGoalOptionsProps {
  targetAmountField: UseFieldReturnType<number, "input", "controlled">;
  applyAccountAmountField: UseFieldReturnType<boolean, "input", "controlled">;
}

const SaveGoalOptions = (props: SaveGoalOptionsProps): React.ReactNode => {
  const { t } = useTranslation();
  const { thousandsSeparator, decimalSeparator, currencySymbol } = useLocale();

  return (
    <Stack gap={"0.5rem"}>
      <NumberInput
        label={<PrimaryText size="sm">{t("target_amount")}</PrimaryText>}
        placeholder={t("enter_target_amount")}
        prefix={currencySymbol}
        min={0}
        decimalScale={2}
        thousandSeparator={thousandsSeparator}
        decimalSeparator={decimalSeparator}
        {...props.targetAmountField.getInputProps()}
        elevation={1}
      />
      <Switch
        label={
          <DimmedText size="sm">
            {t("apply_existing_account_amount_to_goal")}
          </DimmedText>
        }
        checked={props.applyAccountAmountField.getValue()}
        onChange={(event) =>
          props.applyAccountAmountField.setValue(event.currentTarget.checked)
        }
      />
    </Stack>
  );
};

export default SaveGoalOptions;
