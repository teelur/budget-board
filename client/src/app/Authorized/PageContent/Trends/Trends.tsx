import classes from "./Trends.module.css";

import { Stack, Tabs } from "@mantine/core";
import React from "react";
import TransactionsTab from "./TransactionsTab/TransactionsTab";
import AccountsTab from "./AccountsTab/AccountsTab";
import AssetsTab from "./AssetsTab/AssetsTab";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";

const Trends = (): React.ReactNode => {
  const { t } = useTranslation();

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
            <PrimaryText size="sm">{t("transactions")}</PrimaryText>
          </Tabs.Tab>
          <Tabs.Tab value="accounts">
            <PrimaryText size="sm">{t("accounts")}</PrimaryText>
          </Tabs.Tab>
          <Tabs.Tab value="assets">
            <PrimaryText size="sm">{t("assets")}</PrimaryText>
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
