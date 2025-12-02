import { Skeleton, Stack } from "@mantine/core";
import React from "react";
import NetWorthItem from "./NetWorthItem/NetWorthItem";
import {
  filterVisibleAccounts,
  getAccountsOfTypes,
  sumAccountsTotalBalance,
} from "~/helpers/accounts";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IAccountResponse } from "~/models/account";
import { AxiosResponse } from "axios";
import { filterVisibleAssets, sumAssetsTotalValue } from "~/helpers/assets";
import { IAssetResponse } from "~/models/asset";
import { IUserSettings } from "~/models/userSettings";
import Card from "~/components/Card/Card";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";

const NetWorthCard = (): React.ReactNode => {
  const { request } = useAuth();
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
    <Card w="100%" elevation={1}>
      <Stack gap="0.5rem">
        <PrimaryText size="xl">Net Worth</PrimaryText>
        {accountsQuery.isPending ? (
          <Stack gap="0.5rem">
            <Skeleton height={90} radius="lg" />
            <Skeleton height={65} radius="lg" />
            <Skeleton height={40} radius="lg" />
          </Stack>
        ) : (
          <Stack gap="0.5rem">
            <Card p="0.25rem" elevation={2}>
              <Stack gap={0}>
                <NetWorthItem
                  title="Spending"
                  totalBalance={sumAccountsTotalBalance(
                    getAccountsOfTypes(validAccounts, [
                      "Checking",
                      "Credit Card",
                    ])
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
              </Stack>
            </Card>
            <Card p="0.25rem" elevation={2}>
              <Stack gap={0}>
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
              </Stack>
            </Card>
            <Card p="0.25rem" elevation={2}>
              <Stack gap={0}>
                <NetWorthItem
                  title="Total"
                  totalBalance={
                    sumAccountsTotalBalance(validAccounts) +
                    sumAssetsTotalValue(validAssets)
                  }
                  userCurrency={userSettingsQuery.data?.currency ?? "USD"}
                />
              </Stack>
            </Card>
          </Stack>
        )}
      </Stack>
    </Card>
  );
};

export default NetWorthCard;
