import { Button, Stack } from "@mantine/core";

import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import dayjs from "dayjs";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { getCurrencySymbol } from "~/helpers/currency";
import { translateAxiosError } from "~/helpers/requests";
import { IBalanceCreateRequest } from "~/models/balance";
import SurfaceDateInput from "~/components/core/Input/Surface/SurfaceDateInput/SurfaceDateInput";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import SurfaceNumberInput from "~/components/core/Input/Surface/SurfaceNumberInput/SurfaceNumberInput";
import { useTranslation } from "react-i18next";

interface AddBalanceProps {
  accountId: string;
  currency: string;
}

const AddBalance = (props: AddBalanceProps): React.ReactNode => {
  const dateField = useField<string>({
    initialValue: dayjs().toString(),
  });
  const amountField = useField<string | number>({
    initialValue: 0,
  });

  const { t } = useTranslation();
  const { request } = useAuth();

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
      <SurfaceDateInput
        {...dateField.getInputProps()}
        label={<PrimaryText size="sm">{t("date")}</PrimaryText>}
      />
      <SurfaceNumberInput
        {...amountField.getInputProps()}
        label={<PrimaryText size="sm">{t("amount")}</PrimaryText>}
        prefix={getCurrencySymbol(props.currency)}
        decimalScale={2}
        thousandSeparator=","
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
