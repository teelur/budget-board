import { Divider, Group, Select, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import React from "react";

export interface ISelectedColumns {
  date: string | null;
  description: string | null;
  category: string | null;
  amount: string | null;
  account: string | null;
  incomeAmount: string | null;
  expenseAmount: string | null;
}

interface ColumnsSelectProps {
  csvHeaders: string[];
  selectedColumns: ISelectedColumns;
  applySelectedColumns: (columns: ISelectedColumns) => void;
  isAmountSplit: boolean;
}

const ColumnsSelect = (props: ColumnsSelectProps): React.ReactNode => {
  const dateColumnField = useField<string | null>({
    initialValue: props.selectedColumns.date,
  });
  const descriptionColumnField = useField<string | null>({
    initialValue: props.selectedColumns.description,
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
  const incomeAmountColumnField = useField<string | null>({
    initialValue: props.selectedColumns.incomeAmount,
  });
  const expenseAmountColumnField = useField<string | null>({
    initialValue: props.selectedColumns.expenseAmount,
  });

  React.useEffect(() => {
    props.applySelectedColumns({
      date: dateColumnField.getValue(),
      description: descriptionColumnField.getValue(),
      category: categoryColumnField.getValue(),
      amount: amountColumnField.getValue(),
      account: accountColumnField.getValue(),
      incomeAmount: incomeAmountColumnField.getValue(),
      expenseAmount: expenseAmountColumnField.getValue(),
    });
  }, [
    dateColumnField.getValue(),
    descriptionColumnField.getValue(),
    categoryColumnField.getValue(),
    amountColumnField.getValue(),
    accountColumnField.getValue(),
    incomeAmountColumnField.getValue(),
    expenseAmountColumnField.getValue(),
  ]);

  return (
    <Stack gap={0}>
      <Divider label="Columns Fields" labelPosition="center" />
      <Group>
        <Select
          label="Date"
          data={props.csvHeaders}
          clearable
          {...dateColumnField.getInputProps()}
        />
        <Select
          label="Description"
          data={props.csvHeaders}
          clearable
          {...descriptionColumnField.getInputProps()}
        />
        <Select
          label="Category"
          data={props.csvHeaders}
          clearable
          {...categoryColumnField.getInputProps()}
        />
        {props.isAmountSplit ? (
          <>
            <Select
              label="Income Amount"
              data={props.csvHeaders}
              clearable
              {...incomeAmountColumnField.getInputProps()}
            />
            <Select
              label="Expense Amount"
              data={props.csvHeaders}
              clearable
              {...expenseAmountColumnField.getInputProps()}
            />
          </>
        ) : (
          <Select
            label="Amount"
            data={props.csvHeaders}
            clearable
            {...amountColumnField.getInputProps()}
          />
        )}
        <Select
          label="Account"
          data={props.csvHeaders}
          clearable
          {...accountColumnField.getInputProps()}
        />
      </Group>
    </Stack>
  );
};

export default ColumnsSelect;
