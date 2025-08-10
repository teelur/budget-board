import classes from "./NetWorthCard.module.css";

import { Card, Skeleton, Stack, Title } from "@mantine/core";
import React from "react";
import NetWorthItem from "./NetWorthItem/NetWorthItem";
import { filterVisibleAccounts } from "~/helpers/accounts";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IAccount } from "~/models/account";
import { AxiosResponse } from "axios";

const NetWorthCard = (): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);
  const accountsQuery = useQuery({
    queryKey: ["accounts"],
    queryFn: async (): Promise<IAccount[]> => {
      const res: AxiosResponse = await request({
        url: "/api/account",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IAccount[];
      }

      return [];
    },
  });

  const validAccounts = filterVisibleAccounts(accountsQuery.data ?? []);

  return (
    <Card
      className={classes.card}
      w="100%"
      padding="xs"
      radius="md"
      shadow="sm"
      withBorder
    >
      <Stack className={classes.content}>
        <Title order={3}>Net Worth</Title>
        {accountsQuery.isPending ? (
          <Stack gap="0.5rem">
            <Skeleton height={90} radius="lg" />
            <Skeleton height={65} radius="lg" />
            <Skeleton height={40} radius="lg" />
          </Stack>
        ) : (
          <Stack gap="0.5rem">
            <Card
              className={classes.group}
              bg="var(--mantine-color-card-alternate)"
              radius="lg"
              shadow="none"
            >
              <NetWorthItem
                accounts={validAccounts}
                types={["Checking", "Credit Card"]}
                title="Spending"
              />
              <NetWorthItem
                accounts={validAccounts}
                types={["Loan"]}
                title="Loans"
              />
              <NetWorthItem
                accounts={validAccounts}
                types={["Savings"]}
                title="Savings"
              />
            </Card>
            <Card
              className={classes.group}
              bg="var(--mantine-color-card-alternate)"
              radius="lg"
              shadow="none"
            >
              <NetWorthItem
                accounts={validAccounts}
                types={["Checking", "Credit Card", "Loan", "Savings"]}
                title="Liquid"
              />
              <NetWorthItem
                accounts={validAccounts}
                types={["Investment"]}
                title="Investments"
              />
            </Card>
            <Card
              className={classes.group}
              bg="var(--mantine-color-card-alternate)"
              radius="lg"
              shadow="none"
            >
              <NetWorthItem accounts={validAccounts} title="Total" />
            </Card>
          </Stack>
        )}
      </Stack>
    </Card>
  );
};

export default NetWorthCard;
