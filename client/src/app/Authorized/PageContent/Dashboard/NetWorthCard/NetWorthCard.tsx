import classes from "./NetWorthCard.module.css";

import { Card, Skeleton, Stack, Title } from "@mantine/core";
import React from "react";
import NetWorthItem from "./NetWorthItem/NetWorthItem";
import {
  filterVisibleAccounts,
  getAccountsOfTypes,
  sumAccountsTotalBalance,
} from "~/helpers/accounts";
import { AuthContext } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IAccountResponse } from "~/models/account";
import { AxiosResponse } from "axios";
import { filterVisibleAssets, sumAssetsTotalValue } from "~/helpers/assets";
import { IAssetResponse } from "~/models/asset";
import { IUserSettings } from "~/models/userSettings";

const NetWorthCard = (): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);
  const accountsQuery = useQuery({
    queryKey: ["accounts"],
    queryFn: async (): Promise<IAccountResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/account",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IAccountResponse[];
      }

      return [];
    },
  });

  const assetsQuery = useQuery({
    queryKey: ["assets"],
    queryFn: async (): Promise<IAssetResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/asset",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IAssetResponse[];
      }

      return [];
    },
  });

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

  const validAccounts = filterVisibleAccounts(accountsQuery.data ?? []);
  const validAssets = filterVisibleAssets(assetsQuery.data ?? []);

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
                title="Spending"
                totalBalance={sumAccountsTotalBalance(
                  getAccountsOfTypes(validAccounts, ["Checking", "Credit Card"])
                )}
                userCurrency={userSettingsQuery.data?.currency ?? "USD"}
              />
              <NetWorthItem
                title="Loans"
                totalBalance={sumAccountsTotalBalance(
                  getAccountsOfTypes(validAccounts, ["Loan"])
                )}
                userCurrency={userSettingsQuery.data?.currency ?? "USD"}
              />
              <NetWorthItem
                title="Savings"
                totalBalance={sumAccountsTotalBalance(
                  getAccountsOfTypes(validAccounts, ["Savings"])
                )}
                userCurrency={userSettingsQuery.data?.currency ?? "USD"}
              />
            </Card>
            <Card
              className={classes.group}
              bg="var(--mantine-color-card-alternate)"
              radius="lg"
              shadow="none"
            >
              <NetWorthItem
                title="Liquid"
                totalBalance={sumAccountsTotalBalance(
                  getAccountsOfTypes(validAccounts, [
                    "Checking",
                    "Credit Card",
                    "Loan",
                    "Savings",
                  ])
                )}
                userCurrency={userSettingsQuery.data?.currency ?? "USD"}
              />
              <NetWorthItem
                title="Investments"
                totalBalance={sumAccountsTotalBalance(
                  getAccountsOfTypes(validAccounts, ["Investment"])
                )}
                userCurrency={userSettingsQuery.data?.currency ?? "USD"}
              />
              <NetWorthItem
                title="Assets"
                totalBalance={sumAssetsTotalValue(validAssets)}
                userCurrency={userSettingsQuery.data?.currency ?? "USD"}
              />
            </Card>
            <Card
              className={classes.group}
              bg="var(--mantine-color-card-alternate)"
              radius="lg"
              shadow="none"
            >
              <NetWorthItem
                title="Total"
                totalBalance={
                  sumAccountsTotalBalance(validAccounts) +
                  sumAssetsTotalValue(validAssets)
                }
                userCurrency={userSettingsQuery.data?.currency ?? "USD"}
              />
            </Card>
          </Stack>
        )}
      </Stack>
    </Card>
  );
};

export default NetWorthCard;
