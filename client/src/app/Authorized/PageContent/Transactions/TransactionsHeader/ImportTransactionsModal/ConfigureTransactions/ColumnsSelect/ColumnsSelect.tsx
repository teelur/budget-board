import { Divider, SimpleGrid, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import { Select, TextInput } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";

export interface ISelectedColumns {
  date: string | null;
  merchantName: string | null;
  category: string | null;
  amount: string | null;
  account: string | null;
  notes: string | null;
  incomeAmount: string | null;
  expenseAmount: string | null;
}

interface ColumnsSelectProps {
  csvHeaders: string[];
  selectedColumns: ISelectedColumns;
  applySelectedColumns: (columns: ISelectedColumns) => void;
  isAmountSplit: boolean;
  isSingleAccount: boolean;
}

const ColumnsSelect = (props: ColumnsSelectProps): React.ReactNode => {
  const dateColumnField = useField<string | null>({
    initialValue: props.selectedColumns.date,
  });
  const merchantNameColumnField = useField<string | null>({
    initialValue: props.selectedColumns.merchantName,
  });
  const categoryColumnField = useField<string | null>({
    initialValue: props.selectedColumns.category,
  });
  const amountColumnField = useField<string | null>({
    initialValue: props.selectedColumns.amount,
  });
  const accountColumnField = useField<string | null>({
    initialValue: props.selectedColumns.account,
  });
  const notesColumnField = useField<string | null>({
    initialValue: props.selectedColumns.notes,
  });
  const incomeAmountColumnField = useField<string | null>({
    initialValue: props.selectedColumns.incomeAmount,
  });
  const expenseAmountColumnField = useField<string | null>({
    initialValue: props.selectedColumns.expenseAmount,
  });

  const { t } = useTranslation();

  const isInitialMount = React.useRef(true);
  React.useEffect(() => {
    if (isInitialMount.current) {
      isInitialMount.current = false;
      return;
    }
    accountColumnField.setValue(null);
  }, [props.isSingleAccount]);

  React.useEffect(() => {
    props.applySelectedColumns({
      date: dateColumnField.getValue(),
      merchantName: merchantNameColumnField.getValue(),
      category: categoryColumnField.getValue(),
      amount: amountColumnField.getValue(),
      account: accountColumnField.getValue(),
      notes: notesColumnField.getValue(),
      incomeAmount: incomeAmountColumnField.getValue(),
      expenseAmount: expenseAmountColumnField.getValue(),
    });
  }, [
    dateColumnField.getValue(),
    merchantNameColumnField.getValue(),
    categoryColumnField.getValue(),
    amountColumnField.getValue(),
    accountColumnField.getValue(),
    notesColumnField.getValue(),
    incomeAmountColumnField.getValue(),
    expenseAmountColumnField.getValue(),
  ]);

  return (
    <Stack gap="md">
      <Divider label={t("columns_fields")} labelPosition="center" />
      <Stack gap="sm">
        <Divider label={t("core_fields")} labelPosition="left" />
        <SimpleGrid cols={{ base: 1, sm: 2, lg: 3 }} spacing="md">
          <Select
            label={t("date")}
            data={props.csvHeaders}
            clearable
            {...dateColumnField.getInputProps()}
          />
          <Select
            label={t("merchant_name")}
            data={props.csvHeaders}
            clearable
            {...merchantNameColumnField.getInputProps()}
          />
          <Select
            label={t("category")}
            data={props.csvHeaders}
            clearable
            {...categoryColumnField.getInputProps()}
          />
          {props.isAmountSplit ? (
            <>
              <Select
                label={t("income_amount")}
                data={props.csvHeaders}
                clearable
                {...incomeAmountColumnField.getInputProps()}
              />
              <Select
                label={t("expense_amount")}
                data={props.csvHeaders}
                clearable
                {...expenseAmountColumnField.getInputProps()}
              />
            </>
          ) : (
            <Select
              label={t("amount")}
              data={props.csvHeaders}
              clearable
              {...amountColumnField.getInputProps()}
            />
          )}
          {props.isSingleAccount ? (
            <TextInput
              label={t("account")}
              placeholder={t("account_name")}
              {...accountColumnField.getInputProps()}
              value={accountColumnField.getValue() ?? ""}
            />
          ) : (
            <Select
              label={t("account")}
              data={props.csvHeaders}
              clearable
              {...accountColumnField.getInputProps()}
            />
          )}
        </SimpleGrid>
      </Stack>
      <Stack gap="sm">
        <Divider label={t("additional_fields")} labelPosition="left" />
        <SimpleGrid cols={{ base: 1, sm: 2 }} spacing="md">
          <Select
            label={t("notes")}
            data={props.csvHeaders}
            clearable
            {...notesColumnField.getInputProps()}
          />
        </SimpleGrid>
      </Stack>
    </Stack>
  );
};

export default ColumnsSelect;
