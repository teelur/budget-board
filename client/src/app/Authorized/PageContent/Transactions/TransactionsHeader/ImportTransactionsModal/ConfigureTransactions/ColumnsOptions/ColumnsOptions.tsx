import { Button, Divider, Group, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import React from "react";
import { useTranslation } from "react-i18next";
import Autocomplete from "~/components/core/Autocomplete/Autocomplete";
import Card from "~/components/core/Card/Card";
import Checkbox from "~/components/core/Checkbox/Checkbox";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import Select from "~/components/core/Select/Select/Select";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";

export interface IFilterByOptions {
  date: boolean;
  merchantName: boolean;
  category: boolean;
  amount: boolean;
  account: boolean;
}

export const dateFormatOptions = [
  { value: "MM/DD/YYYY", label: "MM/DD/YYYY" },
  { value: "MM/DD/YY", label: "MM/DD/YY" },
  { value: "DD/MM/YYYY", label: "DD/MM/YYYY" },
  { value: "DD/MM/YY", label: "DD/MM/YY" },
  { value: "YYYY/MM/DD", label: "YYYY/MM/DD" },
  { value: "YY/MM/DD", label: "YY/MM/DD" },
];

export interface IColumnsOptions {
  dateFormat: string;
  thousandsSeparator: string;
  decimalSeparator: string;
  invertAmount: boolean;
  splitAmountColumn: boolean;
  includeExpensesColumn: boolean;
  expensesColumn: string | null;
  expensesColumnValue: string | null;
  filterDuplicates: boolean;
  filterByOptions: IFilterByOptions;
}

interface ColumnsOptionsProps {
  columnsOptions: IColumnsOptions;
  applyColumnsOptions: (columnsOptions: IColumnsOptions) => void;
  columns: string[];
  getExpensesColumnValues: (column: string) => string[];
  loading: boolean;
}

const ColumnsOptions = (props: ColumnsOptionsProps): React.ReactNode => {
  const dateFormatField = useField<string>({
    initialValue: props.columnsOptions.dateFormat,
  });

  const thousandsSeparatorField = useField<string>({
    initialValue: props.columnsOptions.thousandsSeparator,
  });
  const decimalSeparatorField = useField<string>({
    initialValue: props.columnsOptions.decimalSeparator,
  });

  const invertAmountField = useField<boolean>({
    initialValue: props.columnsOptions.invertAmount,
  });
  const splitAmountField = useField<boolean>({
    initialValue: props.columnsOptions.splitAmountColumn,
  });
  const includeExpensesColumnField = useField<boolean>({
    initialValue: props.columnsOptions.includeExpensesColumn,
  });
  const expensesColumnField = useField<string | null>({
    initialValue: props.columnsOptions.expensesColumn,
  });
  const expensesColumnValueField = useField<string | null>({
    initialValue: props.columnsOptions.expensesColumnValue,
  });
  const filterDuplicatesField = useField<boolean>({
    initialValue: props.columnsOptions.filterDuplicates,
  });

  const filterByDateField = useField<boolean>({
    initialValue: props.columnsOptions.filterByOptions?.date ?? false,
  });
  const filterByMerchantNameField = useField<boolean>({
    initialValue: props.columnsOptions.filterByOptions?.merchantName ?? false,
  });
  const filterByCategoryField = useField<boolean>({
    initialValue: props.columnsOptions.filterByOptions?.category ?? false,
  });
  const filterByAmountField = useField<boolean>({
    initialValue: props.columnsOptions.filterByOptions?.amount ?? false,
  });
  const filterByAccountField = useField<boolean>({
    initialValue: props.columnsOptions.filterByOptions?.account ?? false,
  });

  const { t } = useTranslation();

  React.useEffect(() => {
    if (splitAmountField.getValue()) {
      includeExpensesColumnField.setValue(false);
      expensesColumnField.setValue(null);
      expensesColumnValueField.setValue(null);
    }
  }, [splitAmountField.getValue()]);

  React.useEffect(() => {
    props.applyColumnsOptions({
      dateFormat: dateFormatField.getValue(),
      thousandsSeparator: thousandsSeparatorField.getValue(),
      decimalSeparator: decimalSeparatorField.getValue(),
      invertAmount: invertAmountField.getValue(),
      splitAmountColumn: splitAmountField.getValue(),
      includeExpensesColumn: includeExpensesColumnField.getValue(),
      expensesColumn: includeExpensesColumnField.getValue()
        ? expensesColumnField.getValue()
        : null,
      expensesColumnValue: includeExpensesColumnField.getValue()
        ? expensesColumnValueField.getValue()
        : null,
      filterDuplicates: filterDuplicatesField.getValue(),
      filterByOptions: filterDuplicatesField.getValue()
        ? {
            date: filterByDateField.getValue(),
            merchantName: filterByMerchantNameField.getValue(),
            category: filterByCategoryField.getValue(),
            amount: filterByAmountField.getValue(),
            account: filterByAccountField.getValue(),
          }
        : {
            date: false,
            merchantName: false,
            category: false,
            amount: false,
            account: false,
          },
    });
  }, [
    dateFormatField.getValue(),
    thousandsSeparatorField.getValue(),
    decimalSeparatorField.getValue(),
    invertAmountField.getValue(),
    splitAmountField.getValue(),
    expensesColumnValueField.getValue(),
    filterDuplicatesField.getValue(),
    filterByDateField.getValue(),
    filterByMerchantNameField.getValue(),
    filterByCategoryField.getValue(),
    filterByAmountField.getValue(),
    filterByAccountField.getValue(),
  ]);

  React.useEffect(() => {
    if (!includeExpensesColumnField.getValue()) {
      expensesColumnField.setValue(null);
      expensesColumnValueField.setValue(null);
    }
  }, [includeExpensesColumnField.getValue()]);

  return (
    <Stack>
      <Divider label={t("columns_options")} labelPosition="center" />
      <Group gap="1rem" justify="space-between" align="stretch">
        <Stack gap="0.5rem">
          <PrimaryText size="sm">{t("date_format")}</PrimaryText>
          <Autocomplete
            label={
              <DimmedText size="xs">{t("date_format_description")}</DimmedText>
            }
            data={dateFormatOptions}
            {...dateFormatField.getInputProps()}
            clearable
            maw="250px"
            elevation={0}
          />
          <Button
            mb="0.25rem"
            size="compact-xs"
            variant="outline"
            onClick={() =>
              window.open(
                "https://budgetboard.net/features/importing-data/csv-import#date-format",
                "_blank",
                "noopener,noreferrer"
              )
            }
          >
            {t("examples")}
          </Button>
        </Stack>
        <Stack gap="0.5rem">
          <PrimaryText size="sm">{t("amount_format")}</PrimaryText>
          <Stack gap="0.25rem">
            <TextInput
              label={
                <DimmedText size="xs">{t("thousands_separator")}</DimmedText>
              }
              {...thousandsSeparatorField.getInputProps()}
              maxLength={1}
              minLength={1}
              elevation={0}
            />
            <TextInput
              label={
                <DimmedText size="xs">{t("decimal_separator")}</DimmedText>
              }
              {...decimalSeparatorField.getInputProps()}
              maxLength={1}
              minLength={1}
              elevation={0}
            />
          </Stack>
        </Stack>
        <Stack gap="0.5rem">
          <PrimaryText size="sm">{t("other_options")}</PrimaryText>
          <Stack gap="0.5rem">
            <Checkbox
              checked={invertAmountField.getValue()}
              onChange={(event) => {
                invertAmountField.setValue(event.currentTarget.checked);
              }}
              label={
                <PrimaryText size="sm">{t("invert_amount_values")}</PrimaryText>
              }
              elevation={0}
            />
            <Checkbox
              checked={splitAmountField.getValue()}
              onChange={(event) => {
                splitAmountField.setValue(event.currentTarget.checked);
              }}
              label={
                <PrimaryText size="sm">
                  {t("split_income_expenses_into_separate_columns")}
                </PrimaryText>
              }
              elevation={0}
            />
            {!splitAmountField.getValue() && (
              <Checkbox
                checked={includeExpensesColumnField.getValue()}
                onChange={(event) => {
                  includeExpensesColumnField.setValue(
                    event.currentTarget.checked
                  );
                }}
                label={
                  <PrimaryText size="sm">
                    {t("include_income_expenses_columns")}
                  </PrimaryText>
                }
                elevation={0}
              />
            )}
            <Group>
              <Checkbox
                checked={filterDuplicatesField.getValue()}
                onChange={(event) => {
                  filterDuplicatesField.setValue(event.currentTarget.checked);
                }}
                label={
                  <PrimaryText size="sm">{t("filter_duplicates")}</PrimaryText>
                }
                elevation={0}
              />
            </Group>
          </Stack>
        </Stack>
      </Group>
      <Group w="100%" justify="flex-end">
        {filterDuplicatesField.getValue() && (
          <Card elevation={1}>
            <Stack justify="center" gap="0.5rem">
              <Stack gap="0">
                <PrimaryText size="sm">{t("filter_duplicates")}</PrimaryText>
                <DimmedText size="xs">
                  {t("filter_duplicates_description")}
                </DimmedText>
              </Stack>
              <Group gap="2rem">
                <Checkbox
                  checked={filterByDateField.getValue()}
                  onChange={(event) =>
                    filterByDateField.setValue(event.currentTarget.checked)
                  }
                  label={<PrimaryText size="sm">{t("date")}</PrimaryText>}
                  elevation={1}
                />
                <Checkbox
                  checked={filterByMerchantNameField.getValue()}
                  onChange={(event) =>
                    filterByMerchantNameField.setValue(
                      event.currentTarget.checked
                    )
                  }
                  label={
                    <PrimaryText size="sm">{t("merchant_name")}</PrimaryText>
                  }
                  elevation={1}
                />
                <Checkbox
                  checked={filterByCategoryField.getValue()}
                  onChange={(event) =>
                    filterByCategoryField.setValue(event.currentTarget.checked)
                  }
                  label={<PrimaryText size="sm">{t("category")}</PrimaryText>}
                  elevation={1}
                />
                <Checkbox
                  checked={filterByAmountField.getValue()}
                  onChange={(event) =>
                    filterByAmountField.setValue(event.currentTarget.checked)
                  }
                  label={<PrimaryText size="sm">{t("amount")}</PrimaryText>}
                  elevation={1}
                />
                <Checkbox
                  checked={filterByAccountField.getValue()}
                  onChange={(event) =>
                    filterByAccountField.setValue(event.currentTarget.checked)
                  }
                  label={<PrimaryText size="sm">{t("account")}</PrimaryText>}
                  elevation={1}
                />
              </Group>
            </Stack>
          </Card>
        )}
        {!splitAmountField.getValue() &&
          includeExpensesColumnField.getValue() && (
            <Card elevation={1}>
              <Stack gap="0.5rem">
                <Stack gap={0}>
                  <PrimaryText size="sm">
                    {t("expenses_column_options")}
                  </PrimaryText>
                  <DimmedText size="xs">
                    {t("expenses_column_options_description")}
                  </DimmedText>
                </Stack>
                <Group gap="0.5rem">
                  {includeExpensesColumnField.getValue() && (
                    <Select
                      label={
                        <PrimaryText size="sm">
                          {t("expenses_column")}
                        </PrimaryText>
                      }
                      data={props.columns}
                      clearable
                      {...expensesColumnField.getInputProps()}
                      elevation={0}
                    />
                  )}
                  {includeExpensesColumnField.getValue() &&
                    expensesColumnField.getValue() && (
                      <Select
                        label={
                          <PrimaryText size="sm">
                            {t("expenses_value")}
                          </PrimaryText>
                        }
                        data={
                          props.getExpensesColumnValues(
                            expensesColumnField.getValue() ?? ""
                          ) ?? []
                        }
                        clearable
                        {...expensesColumnValueField.getInputProps()}
                        elevation={0}
                      />
                    )}
                </Group>
              </Stack>
            </Card>
          )}
      </Group>
    </Stack>
  );
};

export default ColumnsOptions;
