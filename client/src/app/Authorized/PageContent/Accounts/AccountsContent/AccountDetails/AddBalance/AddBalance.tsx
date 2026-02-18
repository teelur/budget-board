import { Button, Stack } from "@mantine/core";

import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { getCurrencySymbol } from "~/helpers/currency";
import { translateAxiosError } from "~/helpers/requests";
import { IBalanceCreateRequest } from "~/models/balance";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import { useDate } from "~/providers/DateProvider/DateProvider";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";

interface AddBalanceProps {
  accountId: string;
  currency: string;
}

const AddBalance = (props: AddBalanceProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs, longDateFormat } = useDate();
  const { request } = useAuth();

  const dateField = useField<Date>({
    initialValue: dayjs().toDate(),
  });
  const amountField = useField<string | number>({
    initialValue: 0,
  });

  const queryClient = useQueryClient();
  const doCreateBalance = useMutation({
    mutationFn: async (newBalance: IBalanceCreateRequest) =>
      await request({
        url: "/api/balance",
        method: "POST",
        data: newBalance,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });
      await queryClient.invalidateQueries({
        queryKey: ["balances", props.accountId],
      });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });

  return (
    <Stack gap={10}>
      <DateInput
        {...dateField.getInputProps()}
        label={<PrimaryText size="sm">{t("date")}</PrimaryText>}
        valueFormat={longDateFormat}
        locale={dayjs.locale()}
        elevation={0}
      />
      <NumberInput
        {...amountField.getInputProps()}
        label={<PrimaryText size="sm">{t("amount")}</PrimaryText>}
        prefix={getCurrencySymbol(props.currency)}
        decimalScale={2}
        thousandSeparator=","
        elevation={0}
      />
      <Button
        loading={doCreateBalance.isPending}
        onClick={() =>
          doCreateBalance.mutate({
            accountID: props.accountId,
            amount: Number(amountField.getValue()),
            dateTime: dayjs(dateField.getValue()).toDate(),
          })
        }
      >
        {t("submit")}
      </Button>
    </Stack>
  );
};

export default AddBalance;
