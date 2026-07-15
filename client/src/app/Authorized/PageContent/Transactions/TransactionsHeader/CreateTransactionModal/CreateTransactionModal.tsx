import { ActionIcon, Button, Stack } from "@mantine/core";
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
import TextInput from "~/components/core/Input/TextInput/TextInput";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import { useTranslation } from "react-i18next";
import AccountMultiSelect from "~/components/core/Select/AccountMultiSelect/AccountMultiSelect";
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
  const { preferredCurrency } = useUserSettings();
  const { allTransactionCategories: transactionCategories } =
    useTransactionCategories();
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
  const accountIdsField = useField<string[]>({
    initialValue: [],
    validate: (value) =>
      value && value.length > 0 ? null : t("account_is_required"),
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
        title={
          <PrimaryHeading order={4}>{t("create_transaction")}</PrimaryHeading>
        }
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
            prefix={getCurrencySymbol(preferredCurrency)}
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
