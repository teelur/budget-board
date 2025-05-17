import { Checkbox, Divider, Group, Select, Stack } from "@mantine/core";
import React from "react";

interface ColumnsOptionsProps {
  invertAmount: boolean;
  setInvertAmount: (invert: boolean) => void;
  includeExpensesColumn: boolean;
  setIncludeExpensesColumn: (include: boolean) => void;
  columns: string[];
  expensesColumn: string | null;
  setExpensesColumn: (column: string | null) => void;
  expensesColumnValues: string[];
  expensesColumnValue: string | null;
  setExpensesColumnValue: (value: string | null) => void;
  filterDuplicates: boolean;
  setFilterDuplicates: (filter: boolean) => void;
  handleFilterDuplicates: () => void;
}

const ColumnsOptions = (props: ColumnsOptionsProps): React.ReactNode => {
  return (
    <Stack>
      <Divider label="Columns Options" labelPosition="center" />
      <Stack>
        <Checkbox
          checked={props.invertAmount}
          onChange={(event) => {
            props.setInvertAmount(event.currentTarget.checked);
          }}
          label="Invert amount"
        />
        <Group justify="flex-start" align="center" w="100%">
          <Checkbox
            checked={props.includeExpensesColumn}
            onChange={(event) => {
              props.setIncludeExpensesColumn(event.currentTarget.checked);
            }}
            label="Include income/expenses column"
          />
          {props.includeExpensesColumn && (
            <Select
              label="Expenses column"
              data={props.columns}
              clearable
              value={props.expensesColumn}
              onChange={(value) => {
                props.setExpensesColumn(value ?? "");
              }}
            />
          )}
          {props.expensesColumn && (
            <Select
              label="Expenses value"
              data={props.expensesColumnValues}
              clearable
              value={props.expensesColumnValue}
              onChange={(value) => {
                props.setExpensesColumnValue(value ?? "");
              }}
            />
          )}
        </Group>
        <Checkbox
          checked={props.filterDuplicates}
          onChange={(event) => {
            props.setFilterDuplicates(event.currentTarget.checked);
            props.handleFilterDuplicates();
          }}
          label="Filter duplicates"
        />
      </Stack>
    </Stack>
  );
};

export default ColumnsOptions;
