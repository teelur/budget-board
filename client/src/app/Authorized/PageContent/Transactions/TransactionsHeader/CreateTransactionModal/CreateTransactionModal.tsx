import { Stack } from "@mantine/core";
import {
  Button,
  CategorySelect,
  DateInput,
  Modal,
  NumberInput,
  SegmentedControl,
  Select,
  TagsInput,
  Textarea,
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
import {
  ITransactionCreateRequest,
  ITransactionTransferCreateRequest,
} from "~/models/transaction";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useCreateTransactionMutation } from "~/hooks/mutations/transactions/useCreateTransactionMutation";
import { useCreateTransferMutation } from "~/hooks/mutations/transactions/useCreateTransferMutation";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { useTagSuggestionsQuery } from "~/hooks/queries/useTagSuggestionsQuery";

const CreateTransactionModal = (): React.ReactNode => {
  const [opened, { open, close }] = useDisclosure(false);
  const [tagSearch, setTagSearch] = React.useState("");
  const [mode, setMode] = React.useState<"transaction" | "transfer">(
    "transaction",
  );

  const { t } = useTranslation();
  const {
    dayjs,
    dayjsLocale,
    longDateFormat,
    thousandsSeparator,
    decimalSeparator,
  } = useLocale();
  const { preferredCurrency, decimalPlaces } = useUserSettings();
  const minimumPositiveAmount =
    decimalPlaces > 0 ? 1 / 10 ** decimalPlaces : 1;
  const { allTransactionCategories: transactionCategories } =
    useTransactionCategories();
  const accountsQuery = useAccountsQuery();
  const tagSuggestionsQuery = useTagSuggestionsQuery({
    prefix: tagSearch,
    enabled: opened,
  });
  const createTransactionMutation = useCreateTransactionMutation();
  const createTransferMutation = useCreateTransferMutation();

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
  const sourceAccountIdField = useField<string | null>({
    initialValue: null,
    validate: (value) => (value ? null : t("account_is_required")),
  });
  const destinationAccountIdField = useField<string | null>({
    initialValue: null,
    validate: (value) => (value ? null : t("account_is_required")),
  });
  const notesField = useField<string>({
    initialValue: "",
  });
  const tagsField = useField<string[]>({
    initialValue: [],
  });

  const handleClose = () => {
    dateField.reset();
    dateField.setValue(dayjs().toDate());
    merchantNameField.reset();
    categoryField.reset();
    amountField.reset();
    accountIdField.reset();
    sourceAccountIdField.reset();
    destinationAccountIdField.reset();
    notesField.reset();
    tagsField.reset();
    setTagSearch("");
    setMode("transaction");
    close();
  };

  const selectableAccounts =
    accountsQuery.data
      ?.filter((account) => !account.deleted && !account.hideAccount)
      .sort((a, b) => a.name.localeCompare(b.name)) ?? [];

  const onSubmit = () => {
    dateField.validate();

    if (!dateField.getValue()) {
      return;
    }

    if (mode === "transfer") {
      sourceAccountIdField.validate();
      destinationAccountIdField.validate();
      const amount = Number(amountField.getValue());
      const sourceAccountID = sourceAccountIdField.getValue();
      const destinationAccountID = destinationAccountIdField.getValue();
      const invalidAmount = !Number.isFinite(amount) || amount <= 0;
      amountField.setError(
        invalidAmount ? t("transfer_amount_must_be_positive") : null,
      );

      if (!sourceAccountID || !destinationAccountID || invalidAmount) {
        return;
      }
      if (sourceAccountID === destinationAccountID) {
        destinationAccountIdField.setError(
          t("transfer_accounts_must_be_different"),
        );
        return;
      }

      destinationAccountIdField.setError(null);
      const transfer: ITransactionTransferCreateRequest = {
        date: dayjs(dateField.getValue()!).format("YYYY-MM-DD"),
        merchantName: null,
        amount,
        sourceAccountID,
        destinationAccountID,
        notes: notesField.getValue(),
        tags: tagsField.getValue(),
      };
      createTransferMutation.mutate(transfer, { onSuccess: handleClose });
      return;
    }

    accountIdField.validate();
    amountField.setError(null);
    if (!accountIdField.getValue()) {
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
      notes: notesField.getValue(),
      tags: tagsField.getValue(),
    } as ITransactionCreateRequest);
  };

  return (
    <>
      <Button
        variant="filled"
        color="primary"
        size="compact-sm"
        aria-label={t("create_transaction")}
        onClick={() => {
          setTagSearch("");
          open();
        }}
      >
        <PlusIcon size={22} />
      </Button>
      <Modal
        opened={opened}
        onClose={handleClose}
        title={
          mode === "transfer" ? t("create_transfer") : t("create_transaction")
        }
      >
        <Stack gap="1rem">
          <SegmentedControl
            color="secondary"
            size="sm"
            fullWidth
            value={mode}
            data={[
              { value: "transaction", label: t("transaction") },
              { value: "transfer", label: t("transfer") },
            ]}
            onChange={(value) => {
              setMode(value as "transaction" | "transfer");
              amountField.setError(null);
              destinationAccountIdField.setError(null);
            }}
            aria-label={t("transaction_creation_mode")}
          />
          <Stack gap="0.25rem">
            <DateInput
              label={t("date")}
              placeholder={t("select_a_date")}
              {...dateField.getInputProps()}
              locale={dayjsLocale}
              valueFormat={longDateFormat}
            />
            {mode === "transaction" && (
              <>
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
              </>
            )}
            <NumberInput
              label={t("amount")}
              placeholder={t("enter_amount")}
              prefix={getCurrencySymbol(preferredCurrency)}
              decimalScale={decimalPlaces}
              thousandSeparator={thousandsSeparator}
              decimalSeparator={decimalSeparator}
min={mode === "transfer" ? minimumPositiveAmount : undefined}
              clampBehavior="none"
              allowNegative={mode !== "transfer"}
              {...amountField.getInputProps()}
            />
            {mode === "transaction" ? (
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
            ) : (
              <>
                <Select
                  label={t("source_account")}
                  placeholder={t("select_source_account")}
                  data={
                    selectableAccounts.map((a) => ({
                      value: a.id,
                      label: a.name,
                    })) ?? []
                  }
                  {...sourceAccountIdField.getInputProps()}
                />
                <Select
                  label={t("destination_account")}
                  placeholder={t("select_destination_account")}
                  data={
                    selectableAccounts.map((a) => ({
                      value: a.id,
                      label: a.name,
                    })) ?? []
                  }
                  {...destinationAccountIdField.getInputProps()}
                />
              </>
            )}
            <Textarea
              label={t("notes")}
              placeholder={t("enter_notes")}
              autosize
              minRows={1}
              maxRows={3}
              {...notesField.getInputProps()}
            />
            <TagsInput
              label={t("tags")}
              placeholder={t("add_or_select_tags")}
              value={tagsField.getValue()}
              data={tagSuggestionsQuery.data ?? []}
              onChange={(value) => tagsField.setValue(value)}
              onSearchChange={setTagSearch}
              comboboxProps={{ withinPortal: true }}
              loading={tagSuggestionsQuery.isPending}
            />
          </Stack>
          <Button
            variant="filled"
            color="primary"
            size="compact-sm"
            onClick={onSubmit}
            loading={
              createTransactionMutation.isPending ||
              createTransferMutation.isPending
            }
          >
            {t("submit")}
          </Button>
        </Stack>
      </Modal>
    </>
  );
};

export default CreateTransactionModal;
