import {
  ActionIcon,
  Button,
  NumberInput,
  Popover,
  Stack,
  Text,
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { SplitIcon } from "lucide-react";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import CategorySelect from "~/components/CategorySelect";
import { getIsParentCategory, getParentCategory } from "~/helpers/category";
import { getCurrencySymbol } from "~/helpers/currency";
import { translateAxiosError } from "~/helpers/requests";
import { ICategory } from "~/models/category";
import { ITransactionSplitRequest } from "~/models/transaction";
import { IUserSettings } from "~/models/userSettings";

interface SplitTransactionProps {
  id: string;
  originalAmount: number;
  categories: ICategory[];
}

interface FormValues {
  amount: string | number;
  category: string;
}

const SplitTransaction = (props: SplitTransactionProps): React.ReactNode => {
  const form = useForm<FormValues>({
    mode: "uncontrolled",
    initialValues: {
      amount: 0,
      category: "",
    },
    validate: {
      amount: (value) =>
        value === ""
          ? "Amount is required."
          : Math.abs(value as number) > Math.abs(props.originalAmount)
          ? "Cannot be greater than original amount."
          : null,
      category: (value) => {
        if (value === "") {
          return "Category is required";
        }
      },
    },
  });

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
  const doSplitTransaction = useMutation({
    mutationFn: async (splitTransaction: ITransactionSplitRequest) =>
      await request({
        url: "/api/transaction/split",
        method: "POST",
        data: splitTransaction,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["transactions"] });
      notifications.show({
        message: "Transaction split successfully.",
        color: "green",
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        message: translateAxiosError(error),
        color: "red",
      });
    },
  });

  const submitSplitTransaction = (values: FormValues) => {
    doSplitTransaction.mutate({
      id: props.id,
      amount: values.amount === "" ? 0 : (values.amount as number),
      category: getParentCategory(values.category, props.categories),
      subcategory: getIsParentCategory(values.category, props.categories)
        ? ""
        : values.category,
    });
  };

  return (
    <Popover>
      <Popover.Target>
        <ActionIcon h="100%">
          <SplitIcon size="1rem" />
        </ActionIcon>
      </Popover.Target>
      <Popover.Dropdown style={{ padding: "0.5rem" }}>
        <form onSubmit={form.onSubmit(submitSplitTransaction)}>
          <Stack gap={5}>
            <Text fw={600}>Split Transaction</Text>
            <NumberInput
              {...form.getInputProps("amount")}
              prefix={getCurrencySymbol(userSettingsQuery.data?.currency)}
              decimalScale={2}
              thousandSeparator=","
              label="Amount"
              maw={200}
            />
            <Stack gap={2}>
              <Text size="sm">Category</Text>
              <CategorySelect
                value={form.getValues().category}
                onChange={(val) => form.setFieldValue("category", val)}
                key={form.key("category")}
                categories={props.categories}
              />
            </Stack>
            <Button
              type="submit"
              mt={5}
              size="compact-sm"
              loading={doSplitTransaction.isPending}
            >
              Submit
            </Button>
          </Stack>
        </form>
      </Popover.Dropdown>
    </Popover>
  );
};

export default SplitTransaction;
