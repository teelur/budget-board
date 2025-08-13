import { Flex, Stack, Text, Grid, Select, Divider } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { ArrowRightIcon } from "lucide-react";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { filterVisibleAccounts } from "~/helpers/accounts";
import { IAccount } from "~/models/account";

interface AccountMappingProps {
  accountNameToAccountIdMap: Map<string, string>;
  setAccountNameToAccountIdMap: React.Dispatch<
    React.SetStateAction<Map<string, string>>
  >;
}

const AccountMapping = (props: AccountMappingProps) => {
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

  const filteredAccounts = React.useMemo(
    () =>
      filterVisibleAccounts(accountsQuery.data ?? []).sort((a, b) =>
        a.name.localeCompare(b.name)
      ),
    [accountsQuery.data]
  );

  return (
    <Stack>
      <Divider label="Account Mapping" labelPosition="center" />
      {Array.from(props.accountNameToAccountIdMap.entries()).map(
        ([accountName, _accountId]) => (
          <Grid key={accountName}>
            <Grid.Col span={4}>
              <Text size="md" fw={600}>
                {accountName}
              </Text>
            </Grid.Col>
            <Grid.Col span={3}>
              <Flex justify="center" align="center">
                <ArrowRightIcon size={16} />
              </Flex>
            </Grid.Col>
            <Grid.Col span={5}>
              <Select
                data={filteredAccounts.map((account) => ({
                  value: account.id,
                  label: account.name,
                }))}
                clearable
                placeholder="Select account"
                onChange={(value) =>
                  props.setAccountNameToAccountIdMap((prev) => {
                    const newMap = new Map(prev);
                    newMap.set(accountName, value ?? "");
                    return newMap;
                  })
                }
              />
            </Grid.Col>
          </Grid>
        )
      )}
    </Stack>
  );
};

export default AccountMapping;
