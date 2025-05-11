import { Divider, Group, Select, Stack } from "@mantine/core";

interface ColumnsSelectProps {
  columns: string[];
  date: string | null;
  description: string | null;
  category: string | null;
  amount: string | null;
  account: string | null;
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
        <Select
          label="Amount"
          data={props.columns}
          clearable
          value={props.amount}
          onChange={(value) => props.setColumn("amount", value ?? "")}
        />
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
