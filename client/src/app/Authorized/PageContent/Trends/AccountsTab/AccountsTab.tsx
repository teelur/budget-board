import { Tabs } from "@mantine/core";
import AssetsTab from "./AssetsTab/AssetsTab";
import LiabilitiesTab from "./LiabilitiesTab/LiabilitiesTab";
import NetWorthTab from "./NetWorthTab/NetWorthTab";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";
import Card from "~/components/Card/Card";

const AccountsTab = (): React.ReactNode => {
  return (
    <Card mt="0.5rem" elevation={1}>
      <Tabs
        variant="pills"
        defaultValue="assets"
        keepMounted={false}
        radius="md"
      >
        <Tabs.List grow>
          <Tabs.Tab value="assets">
            <PrimaryText size="sm">Assets</PrimaryText>
          </Tabs.Tab>
          <Tabs.Tab value="liabilities">
            <PrimaryText size="sm">Liabilities</PrimaryText>
          </Tabs.Tab>
          <Tabs.Tab value="netWorth">
            <PrimaryText size="sm">Net Worth</PrimaryText>
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
