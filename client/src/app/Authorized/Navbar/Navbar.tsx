import classes from "./Navbar.module.css";

import {
  ActionIcon,
  Burger,
  Collapse,
  Group,
  ScrollArea,
  Stack,
  Tooltip,
} from "@mantine/core";
import {
  BanknoteArrowDownIcon,
  BanknoteIcon,
  CalculatorIcon,
  ChartNoAxesColumnIncreasingIcon,
  ChevronDownIcon,
  GoalIcon,
  HouseIcon,
  LandmarkIcon,
  LayoutDashboardIcon,
  LogOutIcon,
  PanelLeftCloseIcon,
  PanelLeftOpenIcon,
  SettingsIcon,
} from "lucide-react";
import NavbarLink from "./NavbarLink/NavbarLink";
import { useTranslation } from "react-i18next";
import { useNavigate, useLocation } from "react-router";
import { useLogoutMutation } from "~/hooks/mutations/auth/useLogoutMutation";
import React from "react";

interface NavbarProps {
  isNavbarOpen: boolean;
  toggleNavbar: () => void;
  closeNavbar: () => void;
  isMobile: boolean;
  isNavbarExpanded: boolean;
  toggleNavbarExpanded: () => void;
}

interface NavbarSettingsItem {
  path: string;
  label: string;
}

interface NavbarItem {
  icon: React.ReactNode;
  path: string;
  label: string;
  settings?: NavbarSettingsItem[];
}

const isPathActive = (pathname: string, path: string) =>
  pathname === path || pathname.startsWith(`${path}/`);

const Navbar = (props: NavbarProps) => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();
  const logoutMutation = useLogoutMutation();

  const sidebarItems: NavbarItem[] = [
    {
      icon: <LayoutDashboardIcon color="currentColor" />,
      path: "/dashboard",
      label: t("dashboard"),
    },
    {
      icon: <LandmarkIcon color="currentColor" />,
      path: "/accounts",
      label: t("accounts"),
      settings: [
        { path: "/accounts/settings/account-types", label: t("account_types") },
        { path: "/accounts/settings/deleted", label: t("deleted_accounts") },
      ],
    },
    {
      icon: <HouseIcon color="currentColor" />,
      path: "/assets",
      label: t("assets"),
      settings: [
        { path: "/assets/settings/asset-types", label: t("asset_types") },
        { path: "/assets/settings/deleted", label: t("deleted_assets") },
      ],
    },
    {
      icon: <BanknoteIcon color="currentColor" />,
      path: "/transactions",
      label: t("transactions"),
      settings: [
        { path: "/transactions/settings/categories", label: t("categories") },
        { path: "/transactions/settings/rules", label: t("automatic_rules") },
        {
          path: "/transactions/settings/deleted",
          label: t("deleted_transactions"),
        },
        {
          path: "/transactions/settings/auto-categorizer",
          label: t("auto_categorizer"),
        },
      ],
    },
    {
      icon: <CalculatorIcon color="currentColor" />,
      path: "/budgets",
      label: t("budgets"),
      settings: [{ path: "/budgets/settings", label: t("budget_settings") }],
    },
    {
      icon: <GoalIcon color="currentColor" />,
      path: "/goals",
      label: t("goals"),
    },
    {
      icon: <ChartNoAxesColumnIncreasingIcon color="currentColor" />,
      path: "/trends",
      label: t("trends"),
    },
  ];

  const settingsItem: NavbarItem = {
    icon: <SettingsIcon color="currentColor" />,
    path: "/settings",
    label: t("settings"),
    settings: [
      { path: "/settings/user", label: t("user_settings") },
      { path: "/settings/security", label: t("security") },
      { path: "/settings/advanced", label: t("advanced_settings") },
    ],
  };
  const [expandedGroups, setExpandedGroups] = React.useState<Set<string>>(
    () =>
      new Set(
        [...sidebarItems, settingsItem]
          .filter((item) => item.settings?.length)
          .map((item) => item.path),
      ),
  );
  const showExpandedNav = props.isMobile || props.isNavbarExpanded;

  const navigateTo = (path: string) => {
    navigate(path);
    props.closeNavbar();
  };

  const toggleGroup = (path: string) => {
    setExpandedGroups((groups) => {
      const nextGroups = new Set(groups);

      if (nextGroups.has(path)) {
        nextGroups.delete(path);
      } else {
        nextGroups.add(path);
      }

      return nextGroups;
    });
  };

  const renderNavbarItem = (item: NavbarItem) => {
    const isActive = isPathActive(location.pathname, item.path);
    const hasSettings = Boolean(item.settings?.length);
    const isGroupExpanded = showExpandedNav && expandedGroups.has(item.path);
    const panelId = `navbar-settings-${item.path.replaceAll("/", "-")}`;

    return (
      <Stack key={item.path} gap={0} className={classes.itemGroup}>
        <Group gap={0} wrap="nowrap" className={classes.itemHeader}>
          <NavbarLink
            icon={item.icon}
            label={item.label}
            active={isActive}
            showLabel={showExpandedNav}
            className={hasSettings ? classes.groupLink : undefined}
            onClick={() => navigateTo(item.path)}
          />
          {hasSettings && showExpandedNav && (
            <Tooltip
              label={
                isGroupExpanded
                  ? t("collapse_sidebar_group")
                  : t("expand_sidebar_group")
              }
              position="right"
              transitionProps={{ duration: 0 }}
            >
              <ActionIcon
                variant="subtle"
                size="lg"
                className={classes.groupToggle}
                aria-label={
                  isGroupExpanded
                    ? t("collapse_sidebar_group")
                    : t("expand_sidebar_group")
                }
                aria-expanded={isGroupExpanded}
                aria-controls={panelId}
                onClick={() => toggleGroup(item.path)}
              >
                <ChevronDownIcon
                  size="1rem"
                  className={isGroupExpanded ? classes.chevronOpen : undefined}
                />
              </ActionIcon>
            </Tooltip>
          )}
        </Group>
        {hasSettings && (
          <Collapse expanded={isGroupExpanded} id={panelId}>
            <Stack gap="0.125rem" className={classes.settingsGroup}>
              {item.settings?.map((setting) => (
                <NavbarLink
                  key={setting.path}
                  icon={null}
                  label={setting.label}
                  active={isPathActive(location.pathname, setting.path)}
                  showLabel
                  labelSize="xs"
                  compact
                  onClick={() => navigateTo(setting.path)}
                />
              ))}
            </Stack>
          </Collapse>
        )}
      </Stack>
    );
  };

  return (
    <ScrollArea h="100%" type="never">
      <Stack justify="space-between" mih="100vh" p="6px" w="100%" gap={0}>
        <Stack justify="center" align="center" gap="0.0625rem" w="100%">
          <Burger
            opened={props.isNavbarOpen}
            className={classes.burger}
            m="0.25rem"
            onClick={props.toggleNavbar}
            hiddenFrom="xs"
            size="md"
          />
          <Tooltip
            label={
              props.isNavbarExpanded
                ? t("collapse_sidebar")
                : t("expand_sidebar")
            }
            position="right"
            transitionProps={{ duration: 0 }}
          >
            <ActionIcon
              variant="subtle"
              visibleFrom="xs"
              className={`${classes.sidebarToggle} ${props.isNavbarExpanded ? classes.sidebarToggleExpanded : ""}`}
              aria-label={
                props.isNavbarExpanded
                  ? t("collapse_sidebar")
                  : t("expand_sidebar")
              }
              onClick={props.toggleNavbarExpanded}
            >
              {props.isNavbarExpanded ? (
                <PanelLeftCloseIcon size="1.25rem" />
              ) : (
                <PanelLeftOpenIcon size="1.25rem" />
              )}
            </ActionIcon>
          </Tooltip>
          {sidebarItems.map(renderNavbarItem)}
        </Stack>
        <Stack justify="center" align="center" gap="0.0625rem" w="100%">
          <NavbarLink
            icon={<BanknoteArrowDownIcon color="currentColor" />}
            label={t("external_accounts")}
            active={isPathActive(location.pathname, "/external-accounts")}
            showLabel={showExpandedNav}
            onClick={() => navigateTo("/external-accounts")}
          />
          {renderNavbarItem(settingsItem)}
          <NavbarLink
            icon={<LogOutIcon color="currentColor" />}
            label={t("logout")}
            showLabel={showExpandedNav}
            onClick={() => logoutMutation.mutate()}
          />
        </Stack>
      </Stack>
    </ScrollArea>
  );
};

export default Navbar;
