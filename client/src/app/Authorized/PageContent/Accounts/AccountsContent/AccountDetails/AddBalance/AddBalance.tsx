import { Stack } from "@mantine/core";
import { Button, DateInput, NumberInput } from "@teelur/budget-board-ui";
import { useField } from "@mantine/form";
import React from "react";
import { getCurrencySymbol } from "~/helpers/currency";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useCreateBalanceMutation } from "~/hooks/mutations/balances/useCreateBalanceMutation";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface AddBalanceProps {
  accountId: string;
}

const AddBalance = (props: AddBalanceProps): React.ReactNode => {
  const { t } = useTranslation();
  const {
    dayjs,
    dayjsLocale,
    longDateFormat,
    thousandsSeparator,
    decimalSeparator,
  } = useLocale();
  const { preferredCurrency, decimalPlaces } = useUserSettings();
  const createBalanceMutation = useCreateBalanceMutation({
    accountId: props.accountId,
  });

  const dateField = useField<Date>({
    initialValue: dayjs().toDate(),
  });
  const amountField = useField<string | number>({
    initialValue: 0,
  });

  return (
    <Stack gap={10}>
      <DateInput
        {...dateField.getInputProps()}
        label={t("date")}
        valueFormat={longDateFormat}
        locale={dayjsLocale}
      />
      <NumberInput
        {...amountField.getInputProps()}
        label={t("amount")}
        prefix={getCurrencySymbol(preferredCurrency)}
        decimalScale={decimalPlaces}
        decimalSeparator={decimalSeparator}
        thousandSeparator={thousandsSeparator}
      />
      <Button
        variant="filled"
        color="primary"
        size="compact-sm"
        loading={createBalanceMutation.isPending}
        onClick={() =>
          createBalanceMutation.mutate({
            accountID: props.accountId,
            amount: Number(amountField.getValue()),
            date: dayjs(dateField.getValue()).format("YYYY-MM-DD"),
          })
        }
      >
        {t("submit")}
      </Button>
    </Stack>
  );
};

export default AddBalance;
