import classes from "./Navbar.module.css";

import { Burger, ScrollArea, Stack } from "@mantine/core";
import {
  BanknoteArrowDownIcon,
  BanknoteIcon,
  CalculatorIcon,
  ChartNoAxesColumnIncreasingIcon,
  GoalIcon,
  HouseIcon,
  LandmarkIcon,
  LayoutDashboardIcon,
  LogOutIcon,
  SettingsIcon,
} from "lucide-react";
import NavbarLink from "./NavbarLink/NavbarLink";
import { useQueryClient } from "@tanstack/react-query";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";
import { useTranslation } from "react-i18next";
import { useNavigate, useLocation } from "react-router";

interface NavbarProps {
  isNavbarOpen: boolean;
  toggleNavbar: () => void;
}

const Navbar = (props: NavbarProps) => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();

  const sidebarItems = [
    {
      icon: <LayoutDashboardIcon color="var(--base-color-text-primary)" />,
      path: "/dashboard",
      label: t("dashboard"),
    },
    {
      icon: <LandmarkIcon color="var(--base-color-text-primary)" />,
      path: "/accounts",
      label: t("accounts"),
    },
    {
      icon: <HouseIcon color="var(--base-color-text-primary)" />,
      path: "/assets",
      label: t("assets"),
    },
    {
      icon: <BanknoteIcon color="var(--base-color-text-primary)" />,
      path: "/transactions",
      label: t("transactions"),
    },
    {
      icon: <CalculatorIcon color="var(--base-color-text-primary)" />,
      path: "/budgets",
      label: t("budgets"),
    },
    {
      icon: <GoalIcon color="var(--base-color-text-primary)" />,
      path: "/goals",
      label: t("goals"),
    },
    {
      icon: (
        <ChartNoAxesColumnIncreasingIcon color="var(--base-color-text-primary)" />
      ),
      path: "/trends",
      label: t("trends"),
    },
  ];

  const { request, setIsUserAuthenticated } = useAuth();

  const queryClient = useQueryClient();
  const Logout = (): void => {
    request({
      url: "/api/logout",
      method: "POST",
      data: {},
    })
      .then(() => {
        queryClient.removeQueries();
        localStorage.setItem("isAuthenticated", "false");
        setIsUserAuthenticated(false);
      })
      .catch((error: AxiosError) => {
        notifications.show({
          color: "var(--button-color-destructive)",
          message: translateAxiosError(error),
        });
      });
  };

  const links = sidebarItems.map((link) => (
    <NavbarLink
      {...link}
      key={link.label}
      active={location.pathname.startsWith(link.path)}
      onClick={() => {
        navigate(link.path);
        props.toggleNavbar();
      }}
    />
  ));

  return (
    <ScrollArea h="100%" type="never">
      <Stack justify="space-between" mih="100vh" p="6px">
        <Stack justify="center" align="center" gap={5}>
          <Burger
            opened={props.isNavbarOpen}
            className={classes.burger}
            m="0.25rem"
            onClick={props.toggleNavbar}
            hiddenFrom="xs"
            size="md"
          />
          {links}
        </Stack>
        <Stack justify="center" align="center" gap={5}>
          <NavbarLink
            icon={
              <BanknoteArrowDownIcon color="var(--base-color-text-primary)" />
            }
            label={t("external_accounts")}
            active={location.pathname.startsWith("/external-accounts")}
            onClick={() => {
              navigate("/external-accounts");
              props.toggleNavbar();
            }}
          />
          <NavbarLink
            icon={<SettingsIcon color="var(--base-color-text-primary)" />}
            label={t("settings")}
            active={location.pathname.startsWith("/settings")}
            onClick={() => {
              navigate("/settings");
              props.toggleNavbar();
            }}
          />
          <NavbarLink
            icon={<LogOutIcon color="var(--base-color-text-primary)" />}
            label={t("logout")}
            onClick={Logout}
          />
        </Stack>
      </Stack>
    </ScrollArea>
  );
};

export default Navbar;
