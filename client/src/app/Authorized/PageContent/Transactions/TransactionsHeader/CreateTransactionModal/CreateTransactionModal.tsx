import { ActionIcon, Button, Stack } from "@mantine/core";
import { isNotEmpty, useForm } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { PlusIcon } from "lucide-react";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { getIsParentCategory, getParentCategory } from "~/helpers/category";
import { getCurrencySymbol } from "~/helpers/currency";
import { translateAxiosError } from "~/helpers/requests";
import { AccountSource } from "~/models/account";
import { ITransactionCreateRequest } from "~/models/transaction";
import { IUserSettings } from "~/models/userSettings";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import Modal from "~/components/Modal/Modal";
import TextInput from "~/components/Input/TextInput/TextInput";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";
import CategorySelect from "~/components/Select/CategorySelect/CategorySelect";
import NumberInput from "~/components/Input/NumberInput/NumberInput";
import AccountSelect from "~/components/Select/AccountSelect/AccountSelect";
import DateInput from "~/components/Input/DateInput/DateInput";

interface formValues {
  date: Date | null;
  description: string;
  category: string;
  amount: number | string;
  accountIds: string[];
}

const CreateTransactionModal = (): React.ReactNode => {
  const [opened, { open, close }] = useDisclosure(false);

  const form = useForm<formValues>({
    mode: "uncontrolled",
    initialValues: {
      date: new Date(),
      description: "",
      category: "",
      amount: 0,
      accountIds: [],
    },

    validate: {
      date: (value) => (value ? null : "Date is required"),
      description: isNotEmpty("name is required"),
      accountIds: isNotEmpty("Account is required"),
    },
  });

  const { transactionCategories } = useTransactionCategories();
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
  const doCreateTransaction = useMutation({
    mutationFn: async (newTransaction: ITransactionCreateRequest) =>
      await request({
        url: "/api/transaction",
        method: "POST",
        data: newTransaction,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["transactions"] });
      await queryClient.invalidateQueries({ queryKey: ["balances"] });
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });
    },
    onError: (error: AxiosError) => {
      notifications.show({ message: translateAxiosError(error), color: "red" });
    },
  });

  const onSubmit = (values: formValues) => {
    if (!values.date) {
      notifications.show({ message: "Date is required", color: "red" });
      return;
    }
    if (!values.accountIds || values.accountIds.length === 0) {
      notifications.show({ message: "Account is required", color: "red" });
      return;
    }

    doCreateTransaction.mutate({
      date: values.date,
      merchantName: values.description,
      category: getParentCategory(values.category, transactionCategories),
      subcategory: getIsParentCategory(values.category, transactionCategories)
        ? null
        : values.category,
      amount: values.amount === "" ? 0 : (values.amount as number),
      accountID: values.accountIds[0]!,
      source: AccountSource.Manual,
      syncID: null,
    });
  };

  return (
    <>
      <ActionIcon size="input-sm" onClick={open}>
        <PlusIcon />
      </ActionIcon>
      <Modal
        opened={opened}
        onClose={close}
        title="Create Transaction"
        styles={{
          inner: {
            left: "0",
            right: "0",
            padding: "0 !important",
          },
        }}
      >
        <form onSubmit={form.onSubmit(onSubmit)}>
          <Stack gap={5}>
            <DateInput
              label={<PrimaryText size="sm">Date</PrimaryText>}
              placeholder="Pick date"
              {...form.getInputProps("date")}
              elevation={0}
            />
            <TextInput
              label={<PrimaryText size="sm">Description</PrimaryText>}
              placeholder="Enter amount"
              {...form.getInputProps("description")}
              elevation={0}
            />
            <CategorySelect
              label={<PrimaryText size="sm">Category</PrimaryText>}
              categories={transactionCategories}
              value={form.getValues().category}
              onChange={(val) => form.setFieldValue("category", val)}
              key={form.key("category")}
              withinPortal
              elevation={0}
            />
            <NumberInput
              label={<PrimaryText size="sm">Amount</PrimaryText>}
              placeholder="Enter amount"
              prefix={getCurrencySymbol(userSettingsQuery.data?.currency)}
              decimalScale={2}
              thousandSeparator=","
              {...form.getInputProps("amount")}
              elevation={0}
            />
            <AccountSelect
              label={<PrimaryText size="sm">Account</PrimaryText>}
              selectedAccountIds={form.getValues().accountIds}
              setSelectedAccountIds={(val) =>
                form.setFieldValue("accountIds", val)
              }
              key={form.key("accountIds")}
              maxSelectedValues={1}
              elevation={0}
            />
            <Button
              type="submit"
              mt={5}
              loading={doCreateTransaction.isPending}
            >
              Submit
            </Button>
          </Stack>
        </form>
      </Modal>
    </>
  );
};

export default CreateTransactionModal;
