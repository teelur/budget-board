import { Group, LoadingOverlay, Stack } from "@mantine/core";
import { ActionIcon } from "@teelur/budget-board-ui";
import { useField } from "@mantine/form";
import { PencilIcon, Trash2Icon } from "lucide-react";
import React from "react";
import { getCurrencySymbol } from "~/helpers/currency";
import { IBalanceResponse } from "~/models/balance";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import { useUpdateBalanceMutation } from "~/hooks/mutations/balances/useUpdateBalanceMutation";
import { useDeleteBalanceMutation } from "~/hooks/mutations/balances/useDeleteBalanceMutation";

interface EditableBalanceItemContentProps {
  balance: IBalanceResponse;
  userCurrency: string;
  doUnSelect: () => void;
}

const EditableBalanceItemContent = (
  props: EditableBalanceItemContentProps,
): React.ReactNode => {
  const {
    dayjs,
    longDateFormat,
    dayjsLocale,
    thousandsSeparator,
    decimalSeparator,
  } = useLocale();
  const updateBalanceMutation = useUpdateBalanceMutation({
    accountID: props.balance.accountID,
  });
  const deleteBalanceMutation = useDeleteBalanceMutation({
    accountID: props.balance.accountID,
  });

  const balanceAmountField = useField<string | number | undefined>({
    initialValue: props.balance.amount,
    validateOnBlur: true,
    validate: (balance) => {
      if (balance === undefined || balance === null || isNaN(Number(balance))) {
        return "Amount must be a valid number";
      }
      return null;
    },
  });
  const balanceDateField = useField<Date>({
    initialValue: dayjs(props.balance.date).toDate(),
  });

  return (
    <Group w="100%" gap="0.5rem" wrap="nowrap" align="flex-start">
      <LoadingOverlay
        visible={
          updateBalanceMutation.isPending || deleteBalanceMutation.isPending
        }
      />
      <Stack w="100%" gap="0.5rem">
        <DateInput
          {...balanceDateField.getInputProps()}
          flex="1 1 auto"
          locale={dayjsLocale}
          valueFormat={longDateFormat}
          onChange={(date) => {
            updateBalanceMutation.mutate({
              id: props.balance.id,
              date: dayjs(date).format("YYYY-MM-DD"),
            });
            balanceDateField.getInputProps().onChange(date);
          }}
          elevation={2}
        />
        <NumberInput
          {...balanceAmountField.getInputProps()}
          flex="1 1 auto"
          prefix={getCurrencySymbol(props.userCurrency)}
          thousandSeparator={thousandsSeparator}
          decimalSeparator={decimalSeparator}
          decimalScale={2}
          fixedDecimalScale
          onBlur={() => {
            const { onBlur } = balanceAmountField.getInputProps();
            onBlur();
            updateBalanceMutation.mutate({
              id: props.balance.id,
              amount: Number(balanceAmountField.getValue()),
            });
          }}
          elevation={2}
        />
      </Stack>
      <Group style={{ alignSelf: "stretch" }} gap="0.5rem" wrap="nowrap">
        <ActionIcon
          variant="outline"
          color="primary"
          size="xs"
          h="100%"
          onClick={(e) => {
            e.stopPropagation();
            props.doUnSelect();
          }}
        >
          <PencilIcon size={16} />
        </ActionIcon>
        <ActionIcon
          variant="filled"
          color="error"
          size="xs"
          h="100%"
          onClick={() => deleteBalanceMutation.mutate(props.balance.id)}
        >
          <Trash2Icon size={16} />
        </ActionIcon>
      </Group>
    </Group>
  );
};

export default EditableBalanceItemContent;
