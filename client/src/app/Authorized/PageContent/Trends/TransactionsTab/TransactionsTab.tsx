import { Tabs } from "@mantine/core";
import SpendingTab from "./SpendingTab/SpendingTab";
import NetCashFlowTab from "./NetCashFlowTab/NetCashFlowTab";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";

const TransactionsTab = (): React.ReactNode => {
  const { t } = useTranslation();

  return (
    <Card mt="0.5rem" elevation={1}>
      <Tabs
        variant="pills"
        defaultValue="spending"
        keepMounted={false}
        radius="md"
      >
        <Tabs.List grow>
          <Tabs.Tab value="spending">
            <PrimaryText size="sm">{t("spending")}</PrimaryText>
          </Tabs.Tab>
          <Tabs.Tab value="netCashFlow">
            <PrimaryText size="sm">{t("net_cash_flow")}</PrimaryText>
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
