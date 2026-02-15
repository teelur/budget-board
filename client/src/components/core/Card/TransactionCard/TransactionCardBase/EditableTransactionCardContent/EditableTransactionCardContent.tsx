import classes from "./EditableTransactionCardContent.module.css";

import { ActionIcon, Flex, Group, LoadingOverlay } from "@mantine/core";
import { ITransaction, ITransactionUpdateRequest } from "~/models/transaction";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";
import { AxiosError, AxiosResponse } from "axios";
import { TrashIcon } from "lucide-react";
import { ICategory } from "~/models/category";
import SplitTransaction from "./SplitTransaction/SplitTransaction";
import { useField } from "@mantine/form";
import { getIsParentCategory, getParentCategory } from "~/helpers/category";
import { getCurrencySymbol } from "~/helpers/currency";
import { IUserSettings } from "~/models/userSettings";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import { useTranslation } from "react-i18next";
import { useDate } from "~/providers/DateProvider/DateProvider";

interface EditableTransactionCardProps {
  transaction: ITransaction;
  categories: ICategory[];
  elevation: number;
}

const EditableTransactionCardContent = (
  props: EditableTransactionCardProps,
): React.ReactNode => {
  const dateField = useField<Date>({
    initialValue: props.transaction.date,
  });
  const merchantField = useField<string>({
    initialValue: props.transaction.merchantName ?? "",
  });
  const categoryField = useField<string>({
    initialValue:
      (props.transaction.subcategory ?? "").length > 0
        ? (props.transaction.subcategory ?? "")
        : (props.transaction.category ?? ""),
  });
  const amountField = useField<number>({
    initialValue: props.transaction.amount,
  });

  const { t } = useTranslation();
  const { dayjs, longDateFormat } = useDate();
  const { request } = useAuth();

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

  const queryClient = useQueryClient();
  const doEditTransaction = useMutation({
    mutationFn: async (newTransaction: ITransactionUpdateRequest) =>
      await request({
        url: "/api/transaction",
        method: "PUT",
        data: newTransaction,
      }),
    onMutate: async (variables: ITransactionUpdateRequest) => {
      await queryClient.cancelQueries({ queryKey: ["transactions"] });

      const previousTransactions: ITransaction[] =
        queryClient.getQueryData(["transactions", { getHidden: false }]) ?? [];

      queryClient.setQueryData(
        ["transactions", { getHidden: false }],
        (oldTransactions: ITransaction[]) =>
          oldTransactions.map((oldTransaction) =>
            oldTransaction.id === variables.id
              ? {
                  ...oldTransaction,
                  amount: variables.amount,
                  date: variables.date,
                  category: variables.category,
                  subcategory: variables.subcategory,
                  merchantName: variables.merchantName,
                }
              : oldTransaction,
          ),
      );

      return { previousTransactions };
    },
    onError: (
      error: AxiosError,
      _variables: ITransactionUpdateRequest,
      context,
    ) => {
      queryClient.setQueryData(
        ["transactions", { getHidden: false }],
        context?.previousTransactions ?? [],
      );
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
    onSettled: async () => {
      await queryClient.invalidateQueries({ queryKey: ["transactions"] });
      await queryClient.invalidateQueries({ queryKey: ["balances"] });
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });
    },
  });

  const doDeleteTransaction = useMutation({
    mutationFn: async (id: string) =>
      await request({
        url: "/api/transaction",
        method: "DELETE",
        params: { guid: id },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["transactions"] });
      await queryClient.invalidateQueries({ queryKey: ["balances"] });
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });
    },
  });

  const onDateChange = (val: string | null) => {
    const parsedDate = dayjs(val);

    if (!parsedDate.isValid()) {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: t("invalid_date"),
      });
      return;
    }

    dateField.setValue(parsedDate.toDate());
    handleSubmit();
  };

  const onCategoryChange = (value: string | null) => {
    categoryField.setValue(value ?? "");
    handleSubmit();
  };

  const handleSubmit = () => {
    doEditTransaction.mutate({
      ...props.transaction,
      date: dateField.getValue(),
      merchantName: merchantField.getValue(),
      category: getParentCategory(
        categoryField.getValue() ?? "",
        props.categories,
      ),
      subcategory: getIsParentCategory(
        categoryField.getValue() ?? "",
        props.categories,
      )
        ? ""
        : (categoryField.getValue() ?? ""),
      amount: amountField.getValue(),
    });
  };

  return (
    <>
      <LoadingOverlay
        visible={doEditTransaction.isPending || doDeleteTransaction.isPending}
      />
      <Group wrap="nowrap" gap="2rem">
        <Flex className={classes.content} w="100%" gap="0.5rem" align="center">
          <Flex
            className={classes.dateContainer}
            flex="1 0 auto"
            align="start"
            onClick={(e) => e.stopPropagation()}
          >
            <DateInput
              w="100%"
              {...dateField.getInputProps()}
              valueFormat={longDateFormat}
              locale={dayjs.locale()}
              onChange={onDateChange}
              elevation={props.elevation}
            />
          </Flex>
          <TextInput
            w="100%"
            flex="1 1 auto"
            {...merchantField.getInputProps()}
            value={merchantField.getValue()}
            onBlur={handleSubmit}
            onClick={(e) => e.stopPropagation()}
            elevation={props.elevation}
          />
          <Flex
            className={classes.categoryContainer}
            flex="1 0 auto"
            onClick={(e) => e.stopPropagation()}
          >
            <CategorySelect
              w="100%"
              categories={props.categories}
              value={categoryField.getValue()}
              onChange={onCategoryChange}
              withinPortal
              elevation={props.elevation}
            />
          </Flex>
          <Flex
            className={classes.amountContainer}
            flex="1 0 auto"
            onClick={(e) => e.stopPropagation()}
          >
            <NumberInput
              w="100%"
              {...amountField.getInputProps()}
              onBlur={handleSubmit}
              onClick={(e) => e.stopPropagation()}
              prefix={getCurrencySymbol(userSettingsQuery.data?.currency)}
              decimalScale={2}
              fixedDecimalScale
              elevation={props.elevation}
            />
          </Flex>
        </Flex>
        <Group gap={5} style={{ alignSelf: "stretch", flexWrap: "nowrap" }}>
          <Flex onClick={(e) => e.stopPropagation()} h="100%">
            <SplitTransaction
              categories={props.categories}
              id={props.transaction.id}
              originalAmount={props.transaction.amount}
              elevation={props.elevation}
            />
          </Flex>
          <ActionIcon
            color="var(--button-color-destructive)"
            onClick={(e) => {
              e.stopPropagation();
              doDeleteTransaction.mutate(props.transaction.id);
            }}
            h="100%"
          >
            <TrashIcon size="1rem" />
          </ActionIcon>
        </Group>
      </Group>
    </>
  );
};

export default EditableTransactionCardContent;
