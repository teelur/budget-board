import {
  Card,
  Checkbox,
  Divider,
  Group,
  LoadingOverlay,
  Select,
  Stack,
  Text,
} from "@mantine/core";
import { useField } from "@mantine/form";
import React from "react";

export interface IFilterByOptions {
  date: boolean;
  description: boolean;
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
  const filterByDescriptionField = useField<boolean>({
    initialValue: props.columnsOptions.filterByOptions?.description ?? false,
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

  React.useEffect(() => {
    if (splitAmountField.getValue()) {
      includeExpensesColumnField.reset();
      expensesColumnField.reset();
      expensesColumnValueField.reset();
    }
  }, [splitAmountField.getValue()]);

  React.useEffect(() => {
    props.applyColumnsOptions({
      dateFormat: dateFormatField.getValue(),
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
            description: filterByDescriptionField.getValue(),
            category: filterByCategoryField.getValue(),
            amount: filterByAmountField.getValue(),
            account: filterByAccountField.getValue(),
          }
        : {
            date: false,
            description: false,
            category: false,
            amount: false,
            account: false,
          },
    });
  }, [
    dateFormatField.getValue(),
    invertAmountField.getValue(),
    splitAmountField.getValue(),
    includeExpensesColumnField.getValue(),
    expensesColumnField.getValue(),
    expensesColumnValueField.getValue(),
    filterDuplicatesField.getValue(),
    filterByDateField.getValue(),
    filterByDescriptionField.getValue(),
    filterByCategoryField.getValue(),
    filterByAmountField.getValue(),
    filterByAccountField.getValue(),
  ]);

  return (
    <Stack>
      <LoadingOverlay visible={props.loading} />
      <Divider label="Columns Options" labelPosition="center" />
      <Stack>
        <Select
          label="Date format"
          data={dateFormatOptions}
          {...dateFormatField.getInputProps()}
          maw="150px"
        />
        <Checkbox
          checked={invertAmountField.getValue()}
          onChange={(event) => {
            invertAmountField.setValue(event.currentTarget.checked);
          }}
          label="Invert amount"
        />
        <Checkbox
          checked={splitAmountField.getValue()}
          onChange={(event) => {
            splitAmountField.setValue(event.currentTarget.checked);
          }}
          label="Split income/expenses into separate columns"
        />
        {!splitAmountField.getValue() && (
          <Group justify="flex-start" align="center" w="100%">
            <Checkbox
              checked={includeExpensesColumnField.getValue()}
              onChange={(event) => {
                includeExpensesColumnField.setValue(
                  event.currentTarget.checked
                );
              }}
              label="Include income/expenses column"
            />
            {includeExpensesColumnField.getValue() && (
              <Select
                label="Expenses column"
                data={props.columns}
                clearable
                {...expensesColumnField.getInputProps()}
              />
            )}
            {expensesColumnValueField.getValue() && (
              <Select
                label="Expenses value"
                data={
                  props.getExpensesColumnValues(
                    expensesColumnField.getValue() ?? ""
                  ) ?? []
                }
                clearable
                {...expensesColumnValueField.getInputProps()}
              />
            )}
          </Group>
        )}
        <Group>
          <Checkbox
            checked={filterDuplicatesField.getValue()}
            onChange={(event) => {
              filterDuplicatesField.setValue(event.currentTarget.checked);
            }}
            label="Filter duplicates"
          />
          {filterDuplicatesField.getValue() && (
            <Card p="0.5rem" radius="md" withBorder>
              <Stack justify="center">
                <Text size="sm" fw={600}>
                  Columns to Match
                </Text>
                <Group>
                  <Checkbox
                    checked={filterByDateField.getValue()}
                    onChange={(event) =>
                      filterByDateField.setValue(event.currentTarget.checked)
                    }
                    label="Date"
                  />
                  <Checkbox
                    checked={filterByDescriptionField.getValue()}
                    onChange={(event) =>
                      filterByDescriptionField.setValue(
                        event.currentTarget.checked
                      )
                    }
                    label="Description"
                  />
                  <Checkbox
                    checked={filterByCategoryField.getValue()}
                    onChange={(event) =>
                      filterByCategoryField.setValue(
                        event.currentTarget.checked
                      )
                    }
                    label="Category"
                  />
                  <Checkbox
                    checked={filterByAmountField.getValue()}
                    onChange={(event) =>
                      filterByAmountField.setValue(event.currentTarget.checked)
                    }
                    label="Amount"
                  />
                  <Checkbox
                    checked={filterByAccountField.getValue()}
                    onChange={(event) =>
                      filterByAccountField.setValue(event.currentTarget.checked)
                    }
                    label="Account"
                  />
                </Group>
              </Stack>
            </Card>
          )}
        </Group>
      </Stack>
    </Stack>
  );
};

export default ColumnsOptions;
