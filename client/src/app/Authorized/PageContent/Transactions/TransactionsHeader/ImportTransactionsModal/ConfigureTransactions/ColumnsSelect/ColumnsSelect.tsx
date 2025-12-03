import { Divider, Group, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import React from "react";
import Select from "~/components/core/Select/Select/Select";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";

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
          label={<PrimaryText size="sm">Date</PrimaryText>}
          data={props.csvHeaders}
          clearable
          {...dateColumnField.getInputProps()}
          elevation={0}
        />
        <Select
          label={<PrimaryText size="sm">Description</PrimaryText>}
          data={props.csvHeaders}
          clearable
          {...descriptionColumnField.getInputProps()}
          elevation={0}
        />
        <Select
          label={<PrimaryText size="sm">Category</PrimaryText>}
          data={props.csvHeaders}
          clearable
          {...categoryColumnField.getInputProps()}
          elevation={0}
        />
        {props.isAmountSplit ? (
          <>
            <Select
              label={<PrimaryText size="sm">Income Amount</PrimaryText>}
              data={props.csvHeaders}
              clearable
              {...incomeAmountColumnField.getInputProps()}
              elevation={0}
            />
            <Select
              label={<PrimaryText size="sm">Expense Amount</PrimaryText>}
              data={props.csvHeaders}
              clearable
              {...expenseAmountColumnField.getInputProps()}
              elevation={0}
            />
          </>
        ) : (
          <Select
            label={<PrimaryText size="sm">Amount</PrimaryText>}
            data={props.csvHeaders}
            clearable
            {...amountColumnField.getInputProps()}
            elevation={0}
          />
        )}
        <Select
          label={<PrimaryText size="sm">Account</PrimaryText>}
          data={props.csvHeaders}
          clearable
          {...accountColumnField.getInputProps()}
          elevation={0}
        />
      </Group>
    </Stack>
  );
};

export default ColumnsSelect;
