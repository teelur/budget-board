import { Card, Group, Stack, Text } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { getFormattedCategoryValue } from "~/helpers/category";
import { ICategory } from "~/models/category";
import { ITransaction } from "~/models/transaction";
import { IUserSettings } from "~/models/userSettings";

interface TransactionCardProps {
  transaction: ITransaction;
  categories: ICategory[];
}

const TransactionCard = (props: TransactionCardProps): React.ReactNode => {
  const categoryValue =
    (props.transaction.subcategory ?? "").length > 0
      ? props.transaction.subcategory ?? ""
      : props.transaction.category ?? "";

  const { request } = React.useContext<any>(AuthContext);

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

  return (
    <Card radius="md" shadow="md" p="0.5rem">
      <Stack gap={0}>
        <Text size="xs" fw={500} c="dimmed">
          {new Date(props.transaction.date).toLocaleDateString()}
        </Text>
        <Text size="sm">{props.transaction.merchantName}</Text>
        <Group justify="space-between" align="center" p={0}>
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
              currency: userSettingsQuery.data?.currency ?? "USD",
            })}
          </Text>
        </Group>
      </Stack>
    </Card>
  );
};

export default TransactionCard;
