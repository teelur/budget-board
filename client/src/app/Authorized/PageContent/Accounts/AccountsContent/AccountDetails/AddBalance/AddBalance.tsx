import { Button, NumberInput, Stack, Text } from "@mantine/core";
import { DatePickerInput } from "@mantine/dates";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import dayjs from "dayjs";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { getCurrencySymbol } from "~/helpers/currency";
import { translateAxiosError } from "~/helpers/requests";
import { IBalanceCreateRequest } from "~/models/balance";

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

  const { request } = React.useContext<any>(AuthContext);

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
      notifications.show({ color: "green", message: "Balance added" });
    },
    onError: (error: AxiosError) =>
      notifications.show({ color: "red", message: translateAxiosError(error) }),
  });

  return (
    <Stack gap={10}>
      <DatePickerInput
        {...dateField.getInputProps()}
        label={
          <Text size="sm" fw={600} c="dimmed">
            Date
          </Text>
        }
      />
      <NumberInput
        {...amountField.getInputProps()}
        label={
          <Text size="sm" fw={600} c="dimmed">
            Amount
          </Text>
        }
        prefix={getCurrencySymbol(props.currency)}
        decimalScale={2}
        thousandSeparator=","
      />
      <Button
        type="submit"
        loading={doCreateBalance.isPending}
        onClick={() =>
          doCreateBalance.mutate({
            accountID: props.accountId,
            amount: Number(amountField.getValue()),
            dateTime: dayjs(dateField.getValue()).toDate(),
          })
        }
      >
        Submit
      </Button>
    </Stack>
  );
};

export default AddBalance;
