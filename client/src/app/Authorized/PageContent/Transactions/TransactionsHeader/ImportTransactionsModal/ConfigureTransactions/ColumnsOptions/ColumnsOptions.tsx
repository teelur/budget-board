import { Divider, Group, LoadingOverlay, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import React from "react";
import Card from "~/components/Card/Card";
import Checkbox from "~/components/Checkbox/Checkbox";
import Select from "~/components/Select/Select/Select";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";

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
      includeExpensesColumnField.setValue(false);
      expensesColumnField.setValue(null);
      expensesColumnValueField.setValue(null);
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
    expensesColumnValueField.getValue(),
    filterDuplicatesField.getValue(),
    filterByDateField.getValue(),
    filterByDescriptionField.getValue(),
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
      <LoadingOverlay visible={props.loading} />
      <Divider label="Columns Options" labelPosition="center" />
      <Stack>
        <Select
          label={<PrimaryText size="sm">Date Format</PrimaryText>}
          data={dateFormatOptions}
          {...dateFormatField.getInputProps()}
          maw="150px"
          elevation={0}
        />
        <Checkbox
          checked={invertAmountField.getValue()}
          onChange={(event) => {
            invertAmountField.setValue(event.currentTarget.checked);
          }}
          label={<PrimaryText size="sm">Invert amount values</PrimaryText>}
          elevation={0}
        />
        <Checkbox
          checked={splitAmountField.getValue()}
          onChange={(event) => {
            splitAmountField.setValue(event.currentTarget.checked);
          }}
          label={
            <PrimaryText size="sm">
              Split income/expenses into separate columns
            </PrimaryText>
          }
          elevation={0}
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
              label={
                <PrimaryText size="sm">
                  Include income/expenses column
                </PrimaryText>
              }
              elevation={0}
            />
            {includeExpensesColumnField.getValue() && (
              <Select
                label={<PrimaryText size="sm">Expenses column</PrimaryText>}
                data={props.columns}
                clearable
                {...expensesColumnField.getInputProps()}
                elevation={0}
              />
            )}
            {includeExpensesColumnField.getValue() &&
              expensesColumnField.getValue() && (
                <Select
                  label={<PrimaryText size="sm">Expenses value</PrimaryText>}
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
        )}
        <Group>
          <Checkbox
            checked={filterDuplicatesField.getValue()}
            onChange={(event) => {
              filterDuplicatesField.setValue(event.currentTarget.checked);
            }}
            label={<PrimaryText size="sm">Filter duplicates</PrimaryText>}
            elevation={0}
          />
          {filterDuplicatesField.getValue() && (
            <Card elevation={1}>
              <Stack justify="center">
                <PrimaryText size="sm">Columns to Match</PrimaryText>
                <Group>
                  <Checkbox
                    checked={filterByDateField.getValue()}
                    onChange={(event) =>
                      filterByDateField.setValue(event.currentTarget.checked)
                    }
                    label={<PrimaryText size="sm">Date</PrimaryText>}
                    elevation={1}
                  />
                  <Checkbox
                    checked={filterByDescriptionField.getValue()}
                    onChange={(event) =>
                      filterByDescriptionField.setValue(
                        event.currentTarget.checked
                      )
                    }
                    label={<PrimaryText size="sm">Description</PrimaryText>}
                    elevation={1}
                  />
                  <Checkbox
                    checked={filterByCategoryField.getValue()}
                    onChange={(event) =>
                      filterByCategoryField.setValue(
                        event.currentTarget.checked
                      )
                    }
                    label={<PrimaryText size="sm">Category</PrimaryText>}
                    elevation={1}
                  />
                  <Checkbox
                    checked={filterByAmountField.getValue()}
                    onChange={(event) =>
                      filterByAmountField.setValue(event.currentTarget.checked)
                    }
                    label={<PrimaryText size="sm">Amount</PrimaryText>}
                    elevation={1}
                  />
                  <Checkbox
                    checked={filterByAccountField.getValue()}
                    onChange={(event) =>
                      filterByAccountField.setValue(event.currentTarget.checked)
                    }
                    label={<PrimaryText size="sm">Account</PrimaryText>}
                    elevation={1}
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
