import classes from "./Trends.module.css";

import { Box, Group, Stack } from "@mantine/core";
import React, { useState } from "react";
import { useTranslation } from "react-i18next";
import NavLink from "~/components/ui/SettingsNavLink/SettingsNavLink";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import SpendingTab from "./TransactionsTab/SpendingTab/SpendingTab";
import SpendingCategoriesTab from "./TransactionsTab/SpendingCategoriesTab/SpendingCategoriesTab";
import NetCashFlowTab from "./TransactionsTab/NetCashFlowTab/NetCashFlowTab";
import AccountsAssetsTab from "./AccountsTab/AssetsTab/AssetsTab";
import LiabilitiesTab from "./AccountsTab/LiabilitiesTab/LiabilitiesTab";
import NetWorthTab from "./AccountsTab/NetWorthTab/NetWorthTab";
import ValuesTab from "./AssetsTab/ValuesTab/ValuesTab";

type TrendView =
  | "spending"
  | "spendingCategories"
  | "netCashFlow"
  | "accountAssets"
  | "liabilities"
  | "netWorth"
  | "values";

const Trends = (): React.ReactNode => {
  const { t } = useTranslation();
  const [activeView, setActiveView] = useState<TrendView>("spending");

  const renderContent = () => {
    switch (activeView) {
      case "spending":
        return <SpendingTab />;
      case "spendingCategories":
        return <SpendingCategoriesTab />;
      case "netCashFlow":
        return <NetCashFlowTab />;
      case "accountAssets":
        return <AccountsAssetsTab />;
      case "liabilities":
        return <LiabilitiesTab />;
      case "netWorth":
        return <NetWorthTab />;
      case "values":
        return <ValuesTab />;
    }
  };

  return (
    <Stack className={classes.root} p="0.5rem">
      <Group align="flex-start" gap="md" wrap="wrap">
        <Stack
          w={{ base: "100%", md: "200px" }}
          gap={4}
          style={{
            flexShrink: 0,
          }}
        >
          <DimmedText size="xs" px="0.5rem">
            {t("transactions")}
          </DimmedText>
          <NavLink
            label={t("spending")}
            active={activeView === "spending"}
            onClick={() => setActiveView("spending")}
          />
          <NavLink
            label={t("spending_categories")}
            active={activeView === "spendingCategories"}
            onClick={() => setActiveView("spendingCategories")}
          />
          <NavLink
            label={t("net_cash_flow")}
            active={activeView === "netCashFlow"}
            onClick={() => setActiveView("netCashFlow")}
          />
          <DimmedText size="xs" px="0.5rem" mt="xs">
            {t("accounts")}
          </DimmedText>
          <NavLink
            label={t("assets")}
            active={activeView === "accountAssets"}
            onClick={() => setActiveView("accountAssets")}
          />
          <NavLink
            label={t("liabilities")}
            active={activeView === "liabilities"}
            onClick={() => setActiveView("liabilities")}
          />
          <NavLink
            label={t("net_worth")}
            active={activeView === "netWorth"}
            onClick={() => setActiveView("netWorth")}
          />
          <DimmedText size="xs" px="0.5rem" mt="xs">
            {t("assets")}
          </DimmedText>
          <NavLink
            label={t("values")}
            active={activeView === "values"}
            onClick={() => setActiveView("values")}
          />
        </Stack>
        <Box w={{ base: "100%", sm: "auto" }} style={{ flex: 1, minWidth: 0 }}>
          {renderContent()}
        </Box>
      </Group>
    </Stack>
  );
};

export default Trends;
