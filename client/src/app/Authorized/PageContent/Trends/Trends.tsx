import classes from "./Trends.module.css";

import { Box, Group, Stack } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import { Outlet, useLocation, useNavigate } from "react-router";
import NavLink from "~/components/ui/SettingsNavLink/SettingsNavLink";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";

const Trends = (): React.ReactNode => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();

  const navigateToTrend = (path: string) => {
    navigate(path);
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
            active={location.pathname.endsWith("/spending")}
            onClick={() => navigateToTrend("spending")}
          />
          <NavLink
            label={t("spending_categories")}
            active={location.pathname.endsWith("/spending-categories")}
            onClick={() => navigateToTrend("spending-categories")}
          />
          <NavLink
            label={t("net_cash_flow")}
            active={location.pathname.endsWith("/net-cash-flow")}
            onClick={() => navigateToTrend("net-cash-flow")}
          />
          <NavLink
            label={t("flows")}
            active={location.pathname.endsWith("/flows")}
            onClick={() => navigateToTrend("flows")}
          />
          <DimmedText size="xs" px="0.5rem" mt="xs">
            {t("accounts")}
          </DimmedText>
          <NavLink
            label={t("assets")}
            active={location.pathname.endsWith("/assets")}
            onClick={() => navigateToTrend("assets")}
          />
          <NavLink
            label={t("liabilities")}
            active={location.pathname.endsWith("/liabilities")}
            onClick={() => navigateToTrend("liabilities")}
          />
          <NavLink
            label={t("net_worth")}
            active={location.pathname.endsWith("/net-worth")}
            onClick={() => navigateToTrend("net-worth")}
          />
          <DimmedText size="xs" px="0.5rem" mt="xs">
            {t("assets")}
          </DimmedText>
          <NavLink
            label={t("values")}
            active={location.pathname.endsWith("/values")}
            onClick={() => navigateToTrend("values")}
          />
        </Stack>
        <Box w={{ base: "100%", sm: "auto" }} style={{ flex: 1, minWidth: 0 }}>
          <Outlet />
        </Box>
      </Group>
    </Stack>
  );
};

export default Trends;
