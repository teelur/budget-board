import { Stack } from "@mantine/core";
import { Button } from "@teelur/budget-board-ui";
import { useField } from "@mantine/form";
import React from "react";
import { getCurrencySymbol } from "~/helpers/currency";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { useCreateValueMutation } from "~/hooks/mutations/values/useCreateValueMutation";

interface AddValueProps {
  assetId: string;
}

const AddValue = (props: AddValueProps): React.ReactNode => {
  const { t } = useTranslation();
  const {
    dayjs,
    dayjsLocale,
    longDateFormat,
    thousandsSeparator,
    decimalSeparator,
  } = useLocale();
  const { preferredCurrency, decimalPlaces } = useUserSettings();
  const createValueMutation = useCreateValueMutation({
    assetId: props.assetId,
  });

  const amountField = useField<string | number>({
    initialValue: 0,
  });
  const dateField = useField<Date>({
    initialValue: dayjs().toDate(),
  });

  return (
    <Stack gap={10}>
      <DateInput
        {...dateField.getInputProps()}
        locale={dayjsLocale}
        valueFormat={longDateFormat}
        label={<PrimaryText size="xs">{t("date")}</PrimaryText>}
        maw={400}
        elevation={0}
      />
      <NumberInput
        {...amountField.getInputProps()}
        label={<PrimaryText size="xs">{t("amount")}</PrimaryText>}
        prefix={getCurrencySymbol(preferredCurrency)}
        decimalScale={decimalPlaces}
        thousandSeparator={thousandsSeparator}
        decimalSeparator={decimalSeparator}
        elevation={0}
      />
      <Button
        variant="filled"
        color="primary"
        size="compact-sm"
        loading={createValueMutation.isPending}
        onClick={() =>
          createValueMutation.mutate(
            {
              amount: Number(amountField.getValue()),
              date: dayjs(dateField.getValue()).format("YYYY-MM-DD"),
              assetID: props.assetId,
            },
            {
              onSuccess: () => {
                dateField.reset();
                amountField.reset();
              },
            },
          )
        }
      >
        {t("add_value")}
      </Button>
    </Stack>
  );
};

export default AddValue;
