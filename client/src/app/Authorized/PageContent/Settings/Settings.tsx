import { Box, Group, Stack } from "@mantine/core";
import { HeadingText } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import { Outlet, useLocation } from "react-router";
import { ChevronRightIcon } from "lucide-react";
import { useApplicationUserQuery } from "~/hooks/queries/useApplicationUserQuery";

const Settings = (): React.ReactNode => {
  const { t } = useTranslation();
  const location = useLocation();
  const applicationUserQuery = useApplicationUserQuery();

  const navItems = [
    { path: "user", label: t("user_settings") },
    { path: "security", label: t("security") },
    { path: "advanced", label: t("advanced_settings") },
    ...(applicationUserQuery.data?.isSuperuser
      ? [{ path: "user-management", label: t("user_management") }]
      : []),
  ];

  const activeItem = navItems.find((item) =>
    location.pathname.endsWith(item.path),
  );

  return (
    <Stack w="100%" p="0.5rem">
      <Group gap="xs">
        <HeadingText level={5} tone="heading">
          {t("settings")}
        </HeadingText>
        {activeItem && (
          <>
            <ChevronRightIcon
              size="1rem"
              color="var(--bbui-color-text-primary)"
            />
            <HeadingText level={5} tone="secondary">
              {activeItem.label}
            </HeadingText>
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
