import { Card, Group, Stack, Text } from "@mantine/core";
import { getFormattedCategoryValue } from "~/helpers/category";
import { ICategory } from "~/models/category";
import { ITransaction } from "~/models/transaction";

interface TransactionCardProps {
  transaction: ITransaction;
  categories: ICategory[];
}

const TransactionCard = (props: TransactionCardProps): React.ReactNode => {
  const categoryValue =
    (props.transaction.subcategory ?? "").length > 0
      ? props.transaction.subcategory ?? ""
      : props.transaction.category ?? "";

  return (
    <Card radius="md" shadow="md">
      <Stack gap={0}>
        <Text size="xs" fw={500} c="dimmed">
          {new Date(props.transaction.date).toLocaleDateString()}
        </Text>
        <Text size="sm">{props.transaction.merchantName}</Text>
        <Group justify="space-between" align="center">
          <Text size="sm" fw={600}>
            {getFormattedCategoryValue(categoryValue, props.categories)}
          </Text>
          <Text
            size="md"
            fw={600}
            style={{
              color:
                props.transaction.amount < 0
                  ? "var(--mantine-color-red-6)"
                  : "var(--mantine-color-green-6)",
              fontWeight: 600,
            }}
          >
            {props.transaction.amount.toLocaleString("en-US", {
              style: "currency",
              currency: "USD",
            })}
          </Text>
        </Group>
      </Stack>
    </Card>
  );
};

export default TransactionCard;
