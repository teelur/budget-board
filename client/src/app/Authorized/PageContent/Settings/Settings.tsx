import { Box, Group, Stack } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import { Outlet, useLocation } from "react-router";
import { ChevronRightIcon } from "lucide-react";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import SecondaryHeading from "~/components/core/Heading/SecondaryHeading/SecondaryHeading";

const Settings = (): React.ReactNode => {
  const { t } = useTranslation();
  const location = useLocation();

  const navItems = [
    { path: "user", label: t("user_settings") },
    { path: "security", label: t("security") },
    { path: "advanced", label: t("advanced_settings") },
  ];

  const activeItem = navItems.find((item) =>
    location.pathname.endsWith(item.path),
  );

  return (
    <Stack w="100%" p="0.5rem">
      <Group gap="xs">
        <PrimaryHeading order={5}>{t("settings")}</PrimaryHeading>
        {activeItem && (
          <>
            <ChevronRightIcon
              size="1rem"
              color="var(--base-color-text-dimmed)"
            />
            <SecondaryHeading order={5}>{activeItem.label}</SecondaryHeading>
          </>
        )}
      </Group>
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

export default Settings;
