import classes from "./Trends.module.css";

import { Stack, Tabs, Text } from "@mantine/core";
import React from "react";
import TransactionsTab from "./TransactionsTab/TransactionsTab";
import AccountsTab from "./AccountsTab/AccountsTab";
import AssetsTab from "./AssetsTab/AssetsTab";

const Trends = (): React.ReactNode => {
  return (
    <Stack className={classes.root}>
      <Tabs
        variant="pills"
        defaultValue="transactions"
        keepMounted={false}
        radius="md"
      >
        <Tabs.List grow>
          <Tabs.Tab value="transactions">
            <Text fw={600} size="sm">
              Transactions
            </Text>
          </Tabs.Tab>
          <Tabs.Tab value="accounts">
            <Text fw={600} size="sm">
              Accounts
            </Text>
          </Tabs.Tab>
          <Tabs.Tab value="assets">
            <Text fw={600} size="sm">
              Assets
            </Text>
          </Tabs.Tab>
        </Tabs.List>
        <Tabs.Panel value="transactions">
          <TransactionsTab />
        </Tabs.Panel>
        <Tabs.Panel value="accounts">
          <AccountsTab />
        </Tabs.Panel>
        <Tabs.Panel value="assets">
          <AssetsTab />
        </Tabs.Panel>
      </Tabs>
    </Stack>
  );
};

export default Trends;
