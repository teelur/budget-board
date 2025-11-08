import { Card, Tabs, Text } from "@mantine/core";
import AssetsTab from "./AssetsTab/AssetsTab";
import LiabilitiesTab from "./LiabilitiesTab/LiabilitiesTab";
import NetWorthTab from "./NetWorthTab/NetWorthTab";

const AccountsTab = (): React.ReactNode => {
  return (
    <Card radius="md" withBorder mt="0.5rem" p="0.5rem">
      <Tabs
        variant="pills"
        defaultValue="assets"
        keepMounted={false}
        radius="md"
      >
        <Tabs.List grow>
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
    </Card>
  );
};

export default AccountsTab;
