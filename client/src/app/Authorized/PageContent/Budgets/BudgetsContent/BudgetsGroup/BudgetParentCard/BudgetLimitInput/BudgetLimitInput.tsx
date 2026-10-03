import { Flex } from "@mantine/core";
import { UseFieldReturnType } from "@mantine/form";
import React from "react";
import { useTranslation } from "react-i18next";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import { getCurrencySymbol } from "~/helpers/currency";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface BudgetLimitInputProps {
  field: UseFieldReturnType<number | string>;
  onSave: (limit: number | string) => void;
  min: number;
}

/**
 * The editable limit on a budget card. This is its own component so it can be passed to a
 * <Trans> placeholder without children: Trans wraps placeholders that have children in a
 * component it recreates on every render, which remounts the input on each keystroke, so it
 * loses focus, never fires onBlur (which saves), and lets arrow clicks fall through to the card.
 */
const BudgetLimitInput = (props: BudgetLimitInputProps): React.ReactNode => {
  const { t } = useTranslation();
  const { thousandsSeparator, decimalSeparator } = useLocale();
  const { preferredCurrency } = useUserSettings();

  return (
    <Flex onClick={(e) => e.stopPropagation()}>
      <NumberInput
        {...props.field.getInputProps()}
        onBlur={() => props.onSave(props.field.getValue())}
        thousandSeparator={thousandsSeparator}
        decimalSeparator={decimalSeparator}
        decimalScale={0}
        min={props.min}
        max={999999}
        step={1}
        prefix={getCurrencySymbol(preferredCurrency)}
        placeholder={t("enter_limit")}
        size="xs"
        styles={{
          root: {
            maxWidth: "100px",
          },
          input: {
            padding: "0 10px",
            fontSize: "16px",
          },
        }}
        elevation={1}
      />
    </Flex>
  );
};

export default BudgetLimitInput;
