import {
  Card,
  Checkbox,
  Divider,
  Group,
  Select,
  Stack,
  Text,
} from "@mantine/core";
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
  filterByDate: boolean;
  setFilterByDate: (filter: boolean) => void;
  filterByDescription: boolean;
  setFilterByDescription: (filter: boolean) => void;
  filterByCategory: boolean;
  setFilterByCategory: (filter: boolean) => void;
  filterByAmount: boolean;
  setFilterByAmount: (filter: boolean) => void;
  filterByAccount: boolean;
  setFilterByAccount: (filter: boolean) => void;
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
        <Group>
          <Checkbox
            checked={props.filterDuplicates}
            onChange={(event) => {
              props.setFilterDuplicates(event.currentTarget.checked);
            }}
            label="Filter duplicates"
          />
          {props.filterDuplicates && (
            <Card p="0.5rem" radius="md" withBorder>
              <Stack justify="center">
                <Text size="sm" fw={600}>
                  Columns to Match
                </Text>
                <Group>
                  <Checkbox
                    checked={props.filterByDate}
                    onChange={(event) =>
                      props.setFilterByDate(event.currentTarget.checked)
                    }
                    label="Date"
                  />
                  <Checkbox
                    checked={props.filterByDescription}
                    onChange={(event) =>
                      props.setFilterByDescription(event.currentTarget.checked)
                    }
                    label="Description"
                  />
                  <Checkbox
                    checked={props.filterByCategory}
                    onChange={(event) =>
                      props.setFilterByCategory(event.currentTarget.checked)
                    }
                    label="Category"
                  />
                  <Checkbox
                    checked={props.filterByAmount}
                    onChange={(event) =>
                      props.setFilterByAmount(event.currentTarget.checked)
                    }
                    label="Amount"
                  />
                  <Checkbox
                    checked={props.filterByAccount}
                    onChange={(event) =>
                      props.setFilterByAccount(event.currentTarget.checked)
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
