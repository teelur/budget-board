import { ActionIcon, Button, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
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
import Modal from "~/components/core/Modal/Modal";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import { useTranslation } from "react-i18next";
import AccountMultiSelect from "~/components/core/Select/AccountMultiSelect/AccountMultiSelect";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

const CreateTransactionModal = (): React.ReactNode => {
  const [opened, { open, close }] = useDisclosure(false);

  const { t } = useTranslation();
  const {
    dayjs,
    dayjsLocale,
    longDateFormat,
    thousandsSeparator,
    decimalSeparator,
  } = useLocale();
  const { transactionCategories } = useTransactionCategories();
  const { request } = useAuth();

  const dateField = useField<Date | null>({
    initialValue: dayjs().toDate(),
    validate: (value) => (value ? null : t("date_is_required")),
  });
  const merchantNameField = useField<string>({
    initialValue: "",
  });
  const categoryField = useField<string>({
    initialValue: "",
  });
  const amountField = useField<number | string>({
    initialValue: 0,
  });
  const accountIdsField = useField<string[]>({
    initialValue: [],
    validate: (value) =>
      value && value.length > 0 ? null : t("account_is_required"),
  });

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
      notifications.show({
        message: translateAxiosError(error),
        color: "var(--button-color-destructive)",
      });
    },
  });

  const onSubmit = () => {
    dateField.validate();
    accountIdsField.validate();

    if (
      !dateField.getValue() ||
      !accountIdsField.getValue() ||
      accountIdsField.getValue().length === 0
    ) {
      return;
    }

    doCreateTransaction.mutate({
      date: dateField.getValue()!,
      merchantName: merchantNameField.getValue(),
      category: getParentCategory(
        categoryField.getValue(),
        transactionCategories,
      ),
      subcategory: getIsParentCategory(
        categoryField.getValue(),
        transactionCategories,
      )
        ? null
        : categoryField.getValue(),
      amount:
        amountField.getValue() === "" ? 0 : (amountField.getValue() as number),
      accountID: accountIdsField.getValue()[0]!,
      source: AccountSource.Manual,
      syncID: null,
    } as ITransactionCreateRequest);
  };

  return (
    <>
      <ActionIcon size="input-sm" onClick={open}>
        <PlusIcon />
      </ActionIcon>
      <Modal
        opened={opened}
        onClose={close}
        title={<PrimaryText>{t("create_transaction")}</PrimaryText>}
      >
        <Stack gap="0.25rem">
          <DateInput
            label={<PrimaryText size="sm">{t("date")}</PrimaryText>}
            placeholder={t("select_a_date")}
            {...dateField.getInputProps()}
            locale={dayjsLocale}
            valueFormat={longDateFormat}
            elevation={0}
          />
          <TextInput
            label={<PrimaryText size="sm">{t("merchant_name")}</PrimaryText>}
            placeholder={t("enter_merchant_name")}
            {...merchantNameField.getInputProps()}
            elevation={0}
          />
          <CategorySelect
            label={<PrimaryText size="sm">{t("category")}</PrimaryText>}
            categories={transactionCategories}
            {...categoryField.getInputProps()}
            withinPortal
            elevation={0}
          />
          <NumberInput
            label={<PrimaryText size="sm">{t("amount")}</PrimaryText>}
            placeholder={t("enter_amount")}
            prefix={getCurrencySymbol(userSettingsQuery.data?.currency)}
            decimalScale={2}
            thousandSeparator={thousandsSeparator}
            decimalSeparator={decimalSeparator}
            {...amountField.getInputProps()}
            elevation={0}
          />
          <AccountMultiSelect
            label={<PrimaryText size="sm">{t("account")}</PrimaryText>}
            {...accountIdsField.getInputProps()}
            maxSelectedValues={1}
            elevation={0}
          />
          <Button
            mt="0.25rem"
            onClick={onSubmit}
            loading={doCreateTransaction.isPending}
          >
            {t("submit")}
          </Button>
        </Stack>
      </Modal>
    </>
  );
};

export default CreateTransactionModal;
