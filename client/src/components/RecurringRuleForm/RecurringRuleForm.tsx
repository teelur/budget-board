import { Button, Group, Stack, Switch } from "@mantine/core";
import { useField } from "@mantine/form";
import React from "react";
import { useTranslation } from "react-i18next";
import { getIsParentCategory, getParentCategory } from "~/helpers/category";
import { getCurrencySymbol } from "~/helpers/currency";
import { useCreateRecurringRuleMutation } from "~/hooks/mutations/recurringRules/useCreateRecurringRuleMutation";
import { useUpdateRecurringRuleMutation } from "~/hooks/mutations/recurringRules/useUpdateRecurringRuleMutation";
import { useAccountsQuery } from "~/hooks/queries/useAccountsQuery";
import {
  defaultRecurringCadence,
  IRecurringRuleResponse,
  RecurringAmountModes,
  RecurringCadenceMode,
  RecurringCadenceModes,
  RecurringCadenceUnit,
  RecurringCadenceUnits,
} from "~/models/recurringRule";
import { ITransaction } from "~/models/transaction";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import Select from "../core/Select/Select/Select";
import DimmedText from "../core/Text/DimmedText/DimmedText";
import {
  createRecurringCadence,
  getRecurringCadenceIntervalMaximum,
  getUpcomingRecurringDates,
} from "~/helpers/recurringRules";

interface RecurringRuleFormProps {
  rule?: IRecurringRuleResponse;
  transaction?: ITransaction;
  onSuccess: () => void;
  onCancel?: () => void;
}

const RecurringRuleForm = (props: RecurringRuleFormProps): React.ReactNode => {
  const { t } = useTranslation();
  const {
    dayjs,
    dayjsLocale,
    longDateFormat,
    thousandsSeparator,
    decimalSeparator,
  } = useLocale();
  const { preferredCurrency } = useUserSettings();
  const { allTransactionCategories } = useTransactionCategories();
  const accountsQuery = useAccountsQuery();
  const createMutation = useCreateRecurringRuleMutation();
  const updateMutation = useUpdateRecurringRuleMutation();

  const sourceRule = props.rule;
  const sourceTransaction = props.transaction;
  const accountField = useField<string>({
    initialValue: sourceRule?.accountID ?? sourceTransaction?.accountID ?? "",
    validate: (value) => (value ? null : t("account_is_required")),
  });
  const merchantNameField = useField<string>({
    initialValue:
      sourceRule?.merchantName ?? sourceTransaction?.merchantName ?? "",
  });
  const categoryField = useField<string>({
    initialValue:
      sourceRule?.subcategory ??
      sourceRule?.category ??
      sourceTransaction?.subcategory ??
      sourceTransaction?.category ??
      "",
  });
  const cadenceUnitField = useField<RecurringCadenceUnit>({
    initialValue: sourceRule?.cadence.unit ?? defaultRecurringCadence.unit,
  });
  const cadenceModeField = useField<RecurringCadenceMode>({
    initialValue: sourceRule?.cadence.mode ?? RecurringCadenceModes.Interval,
  });
  const cadenceIntervalField = useField<number | string>({
    initialValue:
      sourceRule?.cadence.interval ?? defaultRecurringCadence.interval,
    validate: (value) =>
      Number.isInteger(Number(value)) && Number(value) > 0
        ? null
        : t("recurring_interval_required"),
  });
  const startDateField = useField<Date | null>({
    initialValue: dayjs(
      sourceRule?.startDate ??
        sourceTransaction?.date ??
        dayjs().format("YYYY-MM-DD"),
    ).toDate(),
    validate: (value) => (value ? null : t("date_is_required")),
  });
  const endDateField = useField<Date | null>({
    initialValue: sourceRule?.endDate
      ? dayjs(sourceRule.endDate).toDate()
      : null,
  });
  const amountModeField = useField<string>({
    initialValue: sourceRule?.amountMode ?? RecurringAmountModes.Fixed,
  });
  const amountField = useField<number | string>({
    initialValue: sourceRule?.amount ?? sourceTransaction?.amount ?? 0,
    validate: (value) =>
      value !== "" && Number(value) !== 0
        ? null
        : t("recurring_amount_required"),
  });
  const isActiveField = useField<boolean>({
    initialValue: sourceRule?.isActive ?? true,
  });

  const submit = () => {
    accountField.validate();
    startDateField.validate();
    amountField.validate();

    const accountID = accountField.getValue();
    const startDate = startDateField.getValue();
    const amount = amountField.getValue();
    if (!accountID || !startDate || amount === "" || Number(amount) === 0) {
      return;
    }

    const selectedCategory = categoryField.getValue();
    const category = selectedCategory
      ? getParentCategory(selectedCategory, allTransactionCategories)
      : null;
    const subcategory =
      selectedCategory &&
      !getIsParentCategory(selectedCategory, allTransactionCategories)
        ? selectedCategory
        : null;
    const cadenceInterval = Number(cadenceIntervalField.getValue());
    const cadenceMode = cadenceModeField.getValue();
    const cadenceIntervalMaximum = getRecurringCadenceIntervalMaximum(
      cadenceUnitField.getValue(),
      cadenceMode,
    );
    if (
      !Number.isInteger(cadenceInterval) ||
      cadenceInterval <= 0 ||
      (cadenceIntervalMaximum !== undefined &&
        cadenceInterval > cadenceIntervalMaximum)
    ) {
      cadenceIntervalField.validate();
      return;
    }
    const data = {
      accountID,
      merchantName: merchantNameField.getValue() || null,
      category,
      subcategory,
      cadence: createRecurringCadence(
        cadenceUnitField.getValue(),
        cadenceInterval,
        cadenceModeField.getValue(),
      ),
      startDate: dayjs(startDate).format("YYYY-MM-DD"),
      endDate: endDateField.getValue()
        ? dayjs(endDateField.getValue()!).format("YYYY-MM-DD")
        : null,
      isActive: isActiveField.getValue(),
      amountMode:
        amountModeField.getValue() as (typeof RecurringAmountModes)[keyof typeof RecurringAmountModes],
      amount: Number(amount),
    };

    if (sourceRule) {
      updateMutation.mutate(
        { ...data, id: sourceRule.id },
        { onSuccess: props.onSuccess },
      );
      return;
    }

    createMutation.mutate(
      { data, transactionID: sourceTransaction?.id },
      { onSuccess: props.onSuccess },
    );
  };

  const isPending = createMutation.isPending || updateMutation.isPending;
  const cadence = createRecurringCadence(
    cadenceUnitField.getValue(),
    Number(cadenceIntervalField.getValue()),
    cadenceModeField.getValue(),
  );
  const previewDates =
    startDateField.getValue() && cadence.interval > 0
      ? getUpcomingRecurringDates(
          cadence,
          dayjs(startDateField.getValue()!).format("YYYY-MM-DD"),
          endDateField.getValue()
            ? dayjs(endDateField.getValue()!).format("YYYY-MM-DD")
            : null,
        )
      : [];
  const accountOptions = (accountsQuery.data ?? [])
    .filter((account) => account.deleted === null)
    .sort((first, second) => first.name.localeCompare(second.name))
    .map((account) => ({ value: account.id, label: account.name }));

  return (
    <Stack gap="0.5rem">
      <Select
        label={<PrimaryText size="sm">{t("account")}</PrimaryText>}
        placeholder={t("select_account")}
        data={accountOptions}
        searchable
        clearable
        value={accountField.getValue()}
        onChange={(value) => accountField.setValue(value ?? "")}
        error={accountField.error}
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
        categories={allTransactionCategories}
        value={categoryField.getValue() || null}
        onChange={(value) => categoryField.setValue(value)}
        withinPortal
        elevation={0}
      />
      <Select
        label={
          <PrimaryText size="sm">{t("recurring_cadence_mode")}</PrimaryText>
        }
        data={[
          {
            value: RecurringCadenceModes.Interval,
            label: t("recurring_cadence_mode_interval"),
          },
          {
            value: RecurringCadenceModes.PerUnit,
            label: t("recurring_cadence_mode_per_unit"),
          },
        ]}
        {...cadenceModeField.getInputProps()}
        elevation={0}
      />
      <Group grow align="flex-start">
        <Select
          label={<PrimaryText size="sm">{t("recurring_cadence")}</PrimaryText>}
          data={[
            {
              value: RecurringCadenceUnits.Day,
              label: t("recurring_unit_day"),
            },
            {
              value: RecurringCadenceUnits.Week,
              label: t("recurring_unit_week"),
            },
            {
              value: RecurringCadenceUnits.Month,
              label: t("recurring_unit_month"),
            },
            {
              value: RecurringCadenceUnits.Year,
              label: t("recurring_unit_year"),
            },
          ]}
          {...cadenceUnitField.getInputProps()}
          elevation={0}
        />
        <NumberInput
          label={
            <PrimaryText size="sm">
              {t(
                cadenceModeField.getValue() === RecurringCadenceModes.PerUnit
                  ? "recurring_occurrences"
                  : "recurring_interval",
              )}
            </PrimaryText>
          }
          min={1}
          max={getRecurringCadenceIntervalMaximum(
            cadenceUnitField.getValue(),
            cadenceModeField.getValue(),
          )}
          allowDecimal={false}
          allowNegative={false}
          {...cadenceIntervalField.getInputProps()}
          elevation={0}
        />
      </Group>
      {previewDates.length > 0 && (
        <Stack gap="0.25rem">
          <DimmedText size="sm">{t("recurring_upcoming_dates")}</DimmedText>
          <DimmedText size="sm">{previewDates.join(", ")}</DimmedText>
        </Stack>
      )}
      <Group grow>
        <Select
          label={
            <PrimaryText size="sm">{t("recurring_amount_mode")}</PrimaryText>
          }
          data={[
            { value: RecurringAmountModes.Fixed, label: t("fixed_amount") },
            {
              value: RecurringAmountModes.Automatic,
              label: t("automatic_amount"),
            },
          ]}
          {...amountModeField.getInputProps()}
          elevation={0}
        />
      </Group>
      <Group grow>
        <DateInput
          label={<PrimaryText size="sm">{t("start_date")}</PrimaryText>}
          placeholder={t("select_a_date")}
          {...startDateField.getInputProps()}
          locale={dayjsLocale}
          valueFormat={longDateFormat}
          elevation={0}
        />
        <DateInput
          label={<PrimaryText size="sm">{t("end_date")}</PrimaryText>}
          placeholder={t("select_a_date")}
          {...endDateField.getInputProps()}
          locale={dayjsLocale}
          valueFormat={longDateFormat}
          clearable
          elevation={0}
        />
      </Group>
      <NumberInput
        label={<PrimaryText size="sm">{t("amount")}</PrimaryText>}
        description={
          <DimmedText size="xs">{t("recurring_amount_description")}</DimmedText>
        }
        placeholder={t("enter_amount")}
        prefix={getCurrencySymbol(preferredCurrency)}
        decimalScale={2}
        thousandSeparator={thousandsSeparator}
        decimalSeparator={decimalSeparator}
        {...amountField.getInputProps()}
        elevation={0}
      />
      <Switch
        label={
          <PrimaryText size="sm">{t("recurring_rule_active")}</PrimaryText>
        }
        checked={isActiveField.getValue()}
        onChange={(event) =>
          isActiveField.setValue(event.currentTarget.checked)
        }
      />
      <Group grow>
        <Button onClick={submit} loading={isPending}>
          {sourceRule ? t("save") : t("add_recurring_rule")}
        </Button>
        {props.onCancel && (
          <Button
            variant="outline"
            onClick={props.onCancel}
            disabled={isPending}
          >
            {t("cancel")}
          </Button>
        )}
      </Group>
    </Stack>
  );
};

export default RecurringRuleForm;
