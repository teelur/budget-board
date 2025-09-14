import { Flex, Grid, Select, Text } from "@mantine/core";
import { ArrowRightIcon } from "lucide-react";
import React from "react";
import { IAccountItem } from "../AccountMapping";

interface AccountMappingItemProps {
  accountName: string;
  accountId: string;
  accounts: IAccountItem[];
  onAccountChange: (accountName: string, accountId: string) => void;
}

const AccountMappingItem = (
  props: AccountMappingItemProps
): React.ReactNode => {
  const selectedValue =
    props.accounts.find((a) => a.value === props.accountId)?.value ?? null;

  return (
    <Grid>
      <Grid.Col span={4}>
        <Text size="md" fw={600}>
          {props.accountName}
        </Text>
      </Grid.Col>
      <Grid.Col span={3}>
        <Flex justify="center" align="center">
          <ArrowRightIcon size={16} />
        </Flex>
      </Grid.Col>
      <Grid.Col span={5}>
        <Select
          data={props.accounts}
          value={selectedValue}
          clearable
          placeholder="Select account"
          onChange={(value) =>
            props.onAccountChange(props.accountName, value ?? "")
          }
        />
      </Grid.Col>
    </Grid>
  );
};

export default AccountMappingItem;
