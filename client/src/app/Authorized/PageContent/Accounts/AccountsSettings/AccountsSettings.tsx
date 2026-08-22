import { ActionIcon, Box, Group, Stack } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import { Outlet, useLocation, useNavigate } from "react-router";
import { ChevronLeftIcon, ChevronRightIcon } from "lucide-react";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import SecondaryHeading from "~/components/core/Heading/SecondaryHeading/SecondaryHeading";

const AccountsSettings = (): React.ReactNode => {
  const { t } = useTranslation();
  const navigate = useNavigate();
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
      <Group gap="xs">
        <ActionIcon variant="subtle" onClick={() => navigate("/accounts")}>
          <ChevronLeftIcon />
        </ActionIcon>
        <PrimaryHeading order={5}>{t("accounts")}</PrimaryHeading>
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

export default AccountsSettings;
