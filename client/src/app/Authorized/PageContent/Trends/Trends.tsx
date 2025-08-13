import classes from "./Trends.module.css";

import { Stack, Tabs, Text } from "@mantine/core";
import React from "react";
import SpendingTab from "./SpendingTab/SpendingTab";
import NetCashFlowTab from "./NetCashFlowTab/NetCashFlowTab";
import AssetsTab from "./AssetsTab/AssetsTab";
import LiabilitiesTab from "./LiabilitiesTab/LiabilitiesTab";
import NetWorthTab from "./NetWorthTab/NetWorthTab";

const Trends = (): React.ReactNode => {
  return (
    <Stack className={classes.root}>
      <Tabs variant="outline" defaultValue="spending" keepMounted={false}>
        <Tabs.List grow>
          <Tabs.Tab value="spending">
            <Text fw={600} size="sm">
              Spending
            </Text>
          </Tabs.Tab>
          <Tabs.Tab value="netCashFlow">
            <Text fw={600} size="sm">
              Net Cash Flow
            </Text>
          </Tabs.Tab>
          <Tabs.Tab value="assets">
            <Text fw={600} size="sm">
              Assets
            </Text>
          </Tabs.Tab>
          <Tabs.Tab value="liabilities">
            <Text fw={600} size="sm">
              Liabilities
            </Text>
          </Tabs.Tab>
          <Tabs.Tab value="netWorth">
            <Text fw={600} size="sm">
              Net Worth
            </Text>
          </Tabs.Tab>
        </Tabs.List>
        <Tabs.Panel value="spending">
          <SpendingTab />
        </Tabs.Panel>
        <Tabs.Panel value="netCashFlow">
          <NetCashFlowTab />
        </Tabs.Panel>
        <Tabs.Panel value="assets">
          <AssetsTab />
        </Tabs.Panel>
        <Tabs.Panel value="liabilities">
          <LiabilitiesTab />
        </Tabs.Panel>
        <Tabs.Panel value="netWorth">
          <NetWorthTab />
        </Tabs.Panel>
      </Tabs>
    </Stack>
  );
};

export default Trends;
