import { ActionIcon, Group, LoadingOverlay, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import { useDidUpdate } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { PencilIcon, Trash2Icon, Undo2Icon } from "lucide-react";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { getCurrencySymbol } from "~/helpers/currency";
import { translateAxiosError } from "~/helpers/requests";
import { IBalanceResponse, IBalanceUpdateRequest } from "~/models/balance";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";

interface EditableBalanceItemContentProps {
  balance: IBalanceResponse;
  userCurrency: string;
  doUnSelect: () => void;
}

const EditableBalanceItemContent = (
  props: EditableBalanceItemContentProps,
): React.ReactNode => {
  const { t } = useTranslation();
  const {
    dayjs,
    longDateFormat,
    dayjsLocale,
    thousandsSeparator,
    decimalSeparator,
  } = useLocale();
  const { request } = useAuth();

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
    initialValue: props.balance.dateTime,
  });

  const queryClient = useQueryClient();
  const doUpdateBalance = useMutation({
    mutationFn: async () =>
      await request({
        url: `/api/balance`,
        method: "PUT",
        data: {
          id: props.balance.id,
          amount: Number(balanceAmountField.getValue()),
          dateTime: dayjs(balanceDateField.getValue()).toDate(),
        } as IBalanceUpdateRequest,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["balances", props.balance.accountID],
      });
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });

  const doDeleteBalance = useMutation({
    mutationFn: async () =>
      await request({
        url: `/api/balance`,
        method: "DELETE",
        params: { guid: props.balance.id },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["balances", props.balance.accountID],
      });
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });

      notifications.show({
        color: "var(--button-color-confirm)",
        message: t("balance_deleted_successfully_message"),
      });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });

  const doRestoreBalance = useMutation({
    mutationFn: async () =>
      await request({
        url: `/api/balance/restore`,
        method: "POST",
        params: { guid: props.balance.id },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["balances", props.balance.accountID],
      });
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });

      notifications.show({
        color: "var(--button-color-confirm)",
        message: t("balance_restored_successfully_message"),
      });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });

  useDidUpdate(() => {
    doUpdateBalance.mutate();
  }, [balanceDateField.getValue()]);

  return (
    <Group w="100%" gap="0.5rem" wrap="nowrap" align="flex-start">
      <LoadingOverlay
        visible={
          doUpdateBalance.isPending ||
          doDeleteBalance.isPending ||
          doRestoreBalance.isPending
        }
      />
      <Stack w="100%" gap="0.5rem">
        <DateInput
          {...balanceDateField.getInputProps()}
          flex="1 1 auto"
          locale={dayjsLocale}
          valueFormat={longDateFormat}
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
          onBlur={() => doUpdateBalance.mutate()}
          elevation={2}
        />
      </Stack>
      <Group style={{ alignSelf: "stretch" }} gap="0.5rem" wrap="nowrap">
        <ActionIcon
          h="100%"
          variant="outline"
          size="md"
          onClick={(e) => {
            e.stopPropagation();
            props.doUnSelect();
          }}
        >
          <PencilIcon size={16} />
        </ActionIcon>
        {props.balance.deleted ? (
          <ActionIcon
            h="100%"
            size="sm"
            onClick={() => doRestoreBalance.mutate()}
          >
            <Undo2Icon size={16} />
          </ActionIcon>
        ) : (
          <ActionIcon
            h="100%"
            size="sm"
            bg="var(--button-color-destructive)"
            onClick={() => doDeleteBalance.mutate()}
          >
            <Trash2Icon size={16} />
          </ActionIcon>
        )}
      </Group>
    </Group>
  );
};

export default EditableBalanceItemContent;
