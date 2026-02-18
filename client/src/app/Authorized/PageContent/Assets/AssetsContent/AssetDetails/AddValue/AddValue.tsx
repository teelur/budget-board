import { Button, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { getCurrencySymbol } from "~/helpers/currency";
import { IValueCreateRequest, IValueResponse } from "~/models/value";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import { useDate } from "~/providers/DateProvider/DateProvider";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";

interface AddValueProps {
  assetId: string;
  currency: string;
}

const AddValue = (props: AddValueProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs, locale, longDateFormat } = useDate();
  const { request } = useAuth();

  const amountField = useField<string | number>({
    initialValue: 0,
  });
  const dateField = useField<Date>({
    initialValue: dayjs().toDate(),
  });

  const queryClient = useQueryClient();
  const doAddValue = useMutation({
    mutationFn: async (newValue: IValueCreateRequest) => {
      const res = await request({
        url: `/api/value`,
        method: "POST",
        data: newValue,
      });

      if (res.status === 200) {
        return res.data as IValueResponse;
      }

      return undefined;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["assets"] });
      queryClient.invalidateQueries({ queryKey: ["values", props.assetId] });

      amountField.reset();
      dateField.reset();
    },
  });

  return (
    <Stack gap={10}>
      <DateInput
        {...dateField.getInputProps()}
        locale={locale}
        valueFormat={longDateFormat}
        label={<PrimaryText size="xs">{t("date")}</PrimaryText>}
        maw={400}
        elevation={0}
      />
      <NumberInput
        {...amountField.getInputProps()}
        label={<PrimaryText size="xs">{t("amount")}</PrimaryText>}
        prefix={getCurrencySymbol(props.currency)}
        decimalScale={2}
        thousandSeparator=","
        elevation={0}
      />
      <Button
        loading={doAddValue.isPending}
        onClick={() =>
          doAddValue.mutate({
            amount: Number(amountField.getValue()),
            dateTime: dateField.getValue(),
            assetID: props.assetId,
          })
        }
      >
        {t("add_value")}
      </Button>
    </Stack>
  );
};

export default AddValue;
