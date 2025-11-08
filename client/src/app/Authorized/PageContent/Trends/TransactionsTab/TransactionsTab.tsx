import { Card, Tabs, Text } from "@mantine/core";
import SpendingTab from "./SpendingTab/SpendingTab";
import NetCashFlowTab from "./NetCashFlowTab/NetCashFlowTab";

const TransactionsTab = (): React.ReactNode => {
  return (
    <Card radius="md" withBorder mt="0.5rem" p="0.5rem">
      <Tabs
        variant="pills"
        defaultValue="spending"
        keepMounted={false}
        radius="md"
      >
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
        </Tabs.List>
        <Tabs.Panel value="spending">
          <SpendingTab />
        </Tabs.Panel>
        <Tabs.Panel value="netCashFlow">
          <NetCashFlowTab />
        </Tabs.Panel>
      </Tabs>
    </Card>
  );
};

export default TransactionsTab;
