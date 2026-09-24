import { Box, Group, Stack } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import { Outlet, useLocation } from "react-router";
import SettingsHeading from "~/components/SettingsHeading/SettingsHeading";

const TransactionsSettings = (): React.ReactNode => {
  const { t } = useTranslation();
  const location = useLocation();

  const navItems = [
    { path: "categories", label: t("categories") },
    { path: "auto-categorizer", label: t("auto_categorizer") },
    { path: "rules", label: t("automatic_rules") },
    { path: "recurring", label: t("recurring_transactions") },
    { path: "deleted", label: t("deleted_transactions") },
  ];

  const activeItem = navItems.find((item) =>
    location.pathname.endsWith(item.path),
  );

  return (
    <Stack w="100%" p="0.5rem">
      <SettingsHeading
        title={t("transactions")}
        backTo="/transactions"
        activeItem={activeItem?.label}
      />
      <Group align="flex-start" gap="md" wrap="wrap">
        <Box
          w={{ base: "100%", sm: "auto" }}
          maw={800}
          style={{ flex: 1, minWidth: 0 }}
        >
          <Outlet />
        </Box>
      </Group>
    </Stack>
  );
};

export default TransactionsSettings;
