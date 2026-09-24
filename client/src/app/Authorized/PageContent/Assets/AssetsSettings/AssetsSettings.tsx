import { Box, Stack } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import { Outlet, useLocation } from "react-router";
import SettingsHeading from "~/components/SettingsHeading/SettingsHeading";

const AssetsSettings = (): React.ReactNode => {
  const { t } = useTranslation();
  const location = useLocation();

  const navItems = [
    { path: "asset-types", label: t("asset_types") },
    { path: "deleted", label: t("deleted_assets") },
  ];

  const activeItem = navItems.find((item) =>
    location.pathname.endsWith(item.path),
  );

  return (
    <Stack w="100%" p="0.5rem">
      <SettingsHeading
        title={t("assets")}
        backTo="/assets"
        activeItem={activeItem?.label}
      />
      <Box
        w={{ base: "100%", sm: "auto" }}
        maw={800}
        style={{ flex: 1, minWidth: 0 }}
      >
        <Outlet />
      </Box>
    </Stack>
  );
};

export default AssetsSettings;
