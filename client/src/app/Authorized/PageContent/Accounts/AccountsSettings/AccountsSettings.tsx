import { Box, Group, Stack } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import { Outlet, useLocation } from "react-router";
import SettingsHeading from "~/components/SettingsHeading/SettingsHeading";

const AccountsSettings = (): React.ReactNode => {
  const { t } = useTranslation();
  const location = useLocation();

  const navItems = [
    { path: "account-types", label: t("account_types") },
    { path: "deleted", label: t("deleted_accounts") },
  ];

  const activeItem = navItems.find((item) =>
    location.pathname.endsWith(item.path),
  );

  return (
    <Stack w="100%" p="0.5rem">
      <SettingsHeading
        title={t("accounts")}
        backTo="/accounts"
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

export default AccountsSettings;
