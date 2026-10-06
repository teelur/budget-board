import { Stack } from "@mantine/core";
import {
  Button,
  CategorySelect,
  DateInput,
  NumberInput,
  Select,
  TextInput,
} from "@teelur/budget-board-ui";
import { useAccountsQuery } from "~/hooks/queries/useAccountsQuery";
import { useField } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { PlusIcon } from "lucide-react";
import React from "react";
import { getIsParentCategory, getParentCategory } from "~/helpers/category";
import { getCurrencySymbol } from "~/helpers/currency";
import { AccountSource } from "~/models/account";
import { ITransactionCreateRequest } from "~/models/transaction";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import Modal from "~/components/core/Modal/Modal";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import { useCreateTransactionMutation } from "~/hooks/mutations/transactions/useCreateTransactionMutation";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

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
  const { preferredCurrency, decimalPlaces } = useUserSettings();
  const { allTransactionCategories: transactionCategories } =
    useTransactionCategories();
  const accountsQuery = useAccountsQuery();
  const createTransactionMutation = useCreateTransactionMutation();

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
  const accountIdField = useField<string | null>({
    initialValue: null,
    validate: (value) => (value ? null : t("account_is_required")),
  });

  const selectableAccounts =
    accountsQuery.data
      ?.filter((account) => !account.deleted && !account.hideAccount)
      .sort((a, b) => a.name.localeCompare(b.name)) ?? [];

  const onSubmit = () => {
    dateField.validate();
    accountIdField.validate();

    if (!dateField.getValue() || !accountIdField.getValue()) {
      return;
    }

    createTransactionMutation.mutate({
      date: dayjs(dateField.getValue()!).format("YYYY-MM-DD"),
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
      accountID: accountIdField.getValue(),
      source: AccountSource.Manual,
      syncID: null,
    } as ITransactionCreateRequest);
  };

  return (
    <>
      <Button variant="filled" color="primary" size="compact-sm" onClick={open}>
        <PlusIcon size={22} />
      </Button>
      <Modal
        opened={opened}
        onClose={close}
        title={
          <PrimaryHeading order={4}>{t("create_transaction")}</PrimaryHeading>
        }
      >
        <Stack gap="1rem">
          <Stack gap="0.25rem">
            <DateInput
              label={t("date")}
              placeholder={t("select_a_date")}
              {...dateField.getInputProps()}
              locale={dayjsLocale}
              valueFormat={longDateFormat}
            />
            <TextInput
              label={t("merchant_name")}
              placeholder={t("enter_merchant_name")}
              {...merchantNameField.getInputProps()}
            />
            <CategorySelect
              label={t("category")}
              categories={transactionCategories}
              {...categoryField.getInputProps()}
              withinPortal
            />
            <NumberInput
              label={t("amount")}
              placeholder={t("enter_amount")}
              prefix={getCurrencySymbol(preferredCurrency)}
              decimalScale={decimalPlaces}
              thousandSeparator={thousandsSeparator}
              decimalSeparator={decimalSeparator}
              {...amountField.getInputProps()}
            />
            <Select
              label={t("account")}
              placeholder={t("select_an_account")}
              data={
                selectableAccounts.map((a) => ({
                  value: a.id,
                  label: a.name,
                })) ?? []
              }
              {...accountIdField.getInputProps()}
            />
          </Stack>
          <Button
            variant="filled"
            color="primary"
            size="compact-sm"
            onClick={onSubmit}
            loading={createTransactionMutation.isPending}
          >
            {t("submit")}
          </Button>
        </Stack>
      </Modal>
    </>
  );
};

export default CreateTransactionModal;
