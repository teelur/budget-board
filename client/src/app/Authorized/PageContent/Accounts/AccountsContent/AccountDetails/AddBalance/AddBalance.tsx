import { Button, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import React from "react";
import { getCurrencySymbol } from "~/helpers/currency";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import { useCreateBalanceMutation } from "~/hooks/mutations/balances/useCreateBalanceMutation";

interface AddBalanceProps {
  accountId: string;
  currency: string;
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
        label={<PrimaryText size="sm">{t("date")}</PrimaryText>}
        valueFormat={longDateFormat}
        locale={dayjsLocale}
        elevation={0}
      />
      <NumberInput
        {...amountField.getInputProps()}
        label={<PrimaryText size="sm">{t("amount")}</PrimaryText>}
        prefix={getCurrencySymbol(props.currency)}
        decimalScale={2}
        decimalSeparator={decimalSeparator}
        thousandSeparator={thousandsSeparator}
        elevation={0}
      />
      <Button
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
