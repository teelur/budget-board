import { ActionIcon, Box, Group, Stack } from "@mantine/core";
import React from "react";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { Outlet, useLocation, useNavigate } from "react-router";
import { ChevronLeftIcon, ChevronRightIcon } from "lucide-react";
import SettingsNavLink from "~/components/ui/SettingsNavLink/SettingsNavLink";

const TransactionsSettings = (): React.ReactNode => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();

  const navItems = [
    { path: "categories", label: t("custom_categories") },
    { path: "rules", label: t("automatic_rules") },
    { path: "deleted", label: t("deleted_transactions") },
  ];

  const activeItem = navItems.find((item) =>
    location.pathname.endsWith(item.path),
  );

  return (
    <Stack w="100%" p="0.5rem">
      <Group gap="xs">
        <ActionIcon variant="subtle" onClick={() => navigate("/transactions")}>
          <ChevronLeftIcon />
        </ActionIcon>
        <PrimaryText size="lg">{t("transactions_settings")}</PrimaryText>
        {activeItem && (
          <>
            <ChevronRightIcon
              size="1rem"
              color="var(--base-color-text-dimmed)"
            />
            <DimmedText size="lg">{activeItem.label}</DimmedText>
          </>
        )}
      </Group>
      <Group align="flex-start" gap="md" wrap="wrap">
        <Stack
          w={{ base: "100%", sm: "200px" }}
          style={{ flexShrink: 0 }}
          gap={4}
        >
          {navItems.map((item) => (
            <SettingsNavLink
              key={item.path}
              label={item.label}
              active={location.pathname.endsWith(item.path)}
              onClick={() => navigate(item.path)}
            />
          ))}
        </Stack>
        <Box w={{ base: "100%", sm: "auto" }} style={{ flex: 1, minWidth: 0 }}>
          <Outlet />
        </Box>
      </Group>
    </Stack>
  );
};

export default TransactionsSettings;
