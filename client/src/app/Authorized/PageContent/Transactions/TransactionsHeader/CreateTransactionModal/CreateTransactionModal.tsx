import {
  ActionIcon,
  Button,
  Modal,
  NumberInput,
  Stack,
  Text,
  TextInput,
} from "@mantine/core";
import { DatePickerInput } from "@mantine/dates";
import { isNotEmpty, useForm } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { PlusIcon } from "lucide-react";
import React from "react";
import AccountSelectInput from "~/components/AccountSelectInput";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import CategorySelect from "~/components/CategorySelect";
import { getIsParentCategory, getParentCategory } from "~/helpers/category";
import { translateAxiosError } from "~/helpers/requests";
import { AccountSource } from "~/models/account";
import { ICategoryResponse } from "~/models/category";
import {
  defaultTransactionCategories,
  ITransactionCreateRequest,
} from "~/models/transaction";

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

  const { request } = React.useContext<any>(AuthContext);

  const transactionCategoriesQuery = useQuery({
    queryKey: ["transactionCategories"],
    queryFn: async () => {
      const res = await request({
        url: "/api/transactionCategory",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as ICategoryResponse[];
      }

      return undefined;
    },
  });

  const transactionCategoriesWithCustom = defaultTransactionCategories.concat(
    transactionCategoriesQuery.data ?? []
  );

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
      category: getParentCategory(
        values.category,
        transactionCategoriesWithCustom
      ),
      subcategory: getIsParentCategory(
        values.category,
        transactionCategoriesWithCustom
      )
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
      <Modal opened={opened} onClose={close} title="Create Transaction">
        <form onSubmit={form.onSubmit(onSubmit)}>
          <Stack gap={5}>
            <DatePickerInput
              label="Date"
              placeholder="Pick date"
              {...form.getInputProps("date")}
              required
            />
            <TextInput
              label="Description"
              placeholder="Enter amount"
              {...form.getInputProps("description")}
              required
            />
            <Stack gap={3}>
              <Text fz="sm">Category</Text>
              <CategorySelect
                placeholder="Select category"
                required
                categories={transactionCategoriesWithCustom}
                value={form.getValues().category}
                onChange={(val) => form.setFieldValue("category", val)}
                key={form.key("category")}
                withinPortal
              />
            </Stack>
            <NumberInput
              label="Amount"
              placeholder="Enter amount"
              prefix="$"
              decimalScale={2}
              thousandSeparator=","
              {...form.getInputProps("amount")}
              required
            />
            <Stack gap={3}>
              <Text fz="sm">Account</Text>
              <AccountSelectInput
                selectedAccountIds={form.getValues().accountIds}
                setSelectedAccountIds={(val) =>
                  form.setFieldValue("accountIds", val)
                }
                key={form.key("accountIds")}
                hideHidden={false}
                manualOnly
                maxSelectedValues={1}
              />
            </Stack>
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
