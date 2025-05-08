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
}

const ColumnsOptions = (props: ColumnsOptionsProps): React.ReactNode => {
  return (
    <Stack>
      <Divider label="Columns Options" labelPosition="center" />
      <Group w="100%">
        <Stack>
          <Checkbox
            checked={props.invertAmount}
            onChange={(event) =>
              props.setInvertAmount(event.currentTarget.checked)
            }
            label="Invert amount"
          />
          <Checkbox
            checked={props.includeExpensesColumn}
            onChange={(event) =>
              props.setIncludeExpensesColumn(event.currentTarget.checked)
            }
            label="Include income/expenses column"
          />
        </Stack>
        {props.includeExpensesColumn && (
          <Select
            label="Expenses column"
            data={props.columns}
            clearable
            value={props.expensesColumn}
            onChange={(value) => props.setExpensesColumn(value ?? "")}
          />
        )}
        {props.expensesColumn && (
          <Select
            label="Expenses value"
            data={props.expensesColumnValues}
            clearable
            value={props.expensesColumnValue}
            onChange={(value) => props.setExpensesColumnValue(value ?? "")}
          />
        )}
      </Group>
    </Stack>
  );
};

export default ColumnsOptions;
