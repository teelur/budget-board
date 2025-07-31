import classes from "./EditableTransactionCardContent.module.css";

import {
  ActionIcon,
  Flex,
  Group,
  LoadingOverlay,
  NumberInput,
  Stack,
  TextInput,
} from "@mantine/core";
import { ITransaction, ITransactionUpdateRequest } from "~/models/transaction";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";
import { AxiosError, AxiosResponse } from "axios";
import { TrashIcon } from "lucide-react";
import { ICategory } from "~/models/category";
import SplitTransaction from "../SplitTransaction/SplitTransaction";
import { useField } from "@mantine/form";
import { getIsParentCategory, getParentCategory } from "~/helpers/category";
import { DatePickerInput } from "@mantine/dates";
import CategorySelect from "~/components/CategorySelect";
import { getCurrencySymbol } from "~/helpers/currency";
import { IUserSettings } from "~/models/userSettings";
import dayjs from "dayjs";

interface EditableTransactionCardProps {
  transaction: ITransaction;
  categories: ICategory[];
}

const EditableTransactionCardContent = (
  props: EditableTransactionCardProps
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
        ? props.transaction.subcategory ?? ""
        : props.transaction.category ?? "",
  });
  const amountField = useField<number>({
    initialValue: props.transaction.amount,
  });

  const { request } = React.useContext<any>(AuthContext);

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
              : oldTransaction
          )
      );

      return { previousTransactions };
    },
    onError: (
      error: AxiosError,
      _variables: ITransactionUpdateRequest,
      context
    ) => {
      queryClient.setQueryData(
        ["transactions", { getHidden: false }],
        context?.previousTransactions ?? []
      );
      notifications.show({ color: "red", message: translateAxiosError(error) });
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ["transactions"] });
      queryClient.invalidateQueries({ queryKey: ["balances"] });
      queryClient.invalidateQueries({ queryKey: ["accounts"] });
      queryClient.invalidateQueries({ queryKey: ["institutions"] });
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
      queryClient.invalidateQueries({ queryKey: ["balances"] });
      queryClient.invalidateQueries({ queryKey: ["accounts"] });
      queryClient.invalidateQueries({ queryKey: ["institutions"] });
    },
  });

  const onDateChange = (val: string | null) => {
    const parsedDate = dayjs(val);

    if (!parsedDate.isValid()) {
      notifications.show({
        color: "red",
        message: "Invalid date.",
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
        props.categories
      ),
      subcategory: getIsParentCategory(
        categoryField.getValue() ?? "",
        props.categories
      )
        ? ""
        : categoryField.getValue() ?? "",
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
          <Stack
            className={classes.dateContainer}
            flex="1 0 auto"
            align="start"
            onClick={(e) => e.stopPropagation()}
          >
            <DatePickerInput
              w="100%"
              {...dateField.getInputProps()}
              onChange={onDateChange}
            />
          </Stack>
          <TextInput
            w="100%"
            flex="1 1 auto"
            {...merchantField.getInputProps()}
            value={merchantField.getValue()}
            onBlur={handleSubmit}
            onClick={(e) => e.stopPropagation()}
          />
          <Group
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
            />
          </Group>
          <Group
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
            />
          </Group>
        </Flex>
        <Group gap={5} style={{ alignSelf: "stretch", flexWrap: "nowrap" }}>
          <Flex onClick={(e) => e.stopPropagation()} h="100%">
            <SplitTransaction
              categories={props.categories}
              id={props.transaction.id}
              originalAmount={props.transaction.amount}
            />
          </Flex>
          <ActionIcon
            color="red"
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
