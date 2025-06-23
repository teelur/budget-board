import { Divider, Group, Select, Stack } from "@mantine/core";

interface ColumnsSelectProps {
  columns: string[];
  date: string | null;
  description: string | null;
  category: string | null;
  amount: string | null;
  account: string | null;
  incomeAmount: string | null;
  expenseAmount: string | null;
  splitAmount: boolean;
  setColumn: (column: string, value: string) => void;
}

const ColumnsSelect = (props: ColumnsSelectProps): React.ReactNode => {
  return (
    <Stack gap={0}>
      <Divider label="Columns Fields" labelPosition="center" />
      <Group>
        <Select
          label="Date"
          data={props.columns}
          clearable
          value={props.date}
          onChange={(value) => props.setColumn("date", value ?? "")}
        />
        <Select
          label="Description"
          data={props.columns}
          clearable
          value={props.description}
          onChange={(value) => props.setColumn("description", value ?? "")}
        />
        <Select
          label="Category"
          data={props.columns}
          clearable
          value={props.category}
          onChange={(value) => props.setColumn("category", value ?? "")}
        />
        {!props.splitAmount && (
          <Select
            label="Amount"
            data={props.columns}
            clearable
            value={props.amount}
            onChange={(value) => props.setColumn("amount", value ?? "")}
          />
        )}
        {props.splitAmount && (
          <>
            <Select
              label="Income Amount"
              data={props.columns}
              clearable
              value={props.incomeAmount}
              onChange={(value) => props.setColumn("incomeAmount", value ?? "")}
            />
            <Select
              label="Expense Amount"
              data={props.columns}
              clearable
              value={props.expenseAmount}
              onChange={(value) =>
                props.setColumn("expenseAmount", value ?? "")
              }
            />
          </>
        )}
        <Select
          label="Account"
          data={props.columns}
          clearable
          value={props.account}
          onChange={(value) => props.setColumn("account", value ?? "")}
        />
      </Group>
    </Stack>
  );
};

export default ColumnsSelect;
