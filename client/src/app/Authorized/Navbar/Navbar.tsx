import classes from "./Navbar.module.css";

import { Burger, Stack } from "@mantine/core";
import {
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
import NavbarLink from "./NavbarLink";
import { Pages } from "../PageContent/PageContent";
import { useQueryClient } from "@tanstack/react-query";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";
import { useTranslation } from "react-i18next";

interface NavbarProps {
  currentPage: Pages;
  setCurrentPage: (page: Pages) => void;
  isNavbarOpen: boolean;
  toggleNavbar: () => void;
}

const Navbar = (props: NavbarProps) => {
  const { t } = useTranslation();

  const sidebarItems = [
    {
      icon: <LayoutDashboardIcon color="var(--base-color-text-primary)" />,
      page: Pages.Dashboard,
      label: t("dashboard"),
    },
    {
      icon: <LandmarkIcon color="var(--base-color-text-primary)" />,
      page: Pages.Accounts,
      label: t("accounts"),
    },
    {
      icon: <HouseIcon color="var(--base-color-text-primary)" />,
      page: Pages.Assets,
      label: t("assets"),
    },
    {
      icon: <BanknoteIcon color="var(--base-color-text-primary)" />,
      page: Pages.Transactions,
      label: t("transactions"),
    },
    {
      icon: <CalculatorIcon color="var(--base-color-text-primary)" />,
      page: Pages.Budgets,
      label: t("budgets"),
    },
    {
      icon: <GoalIcon color="var(--base-color-text-primary)" />,
      page: Pages.Goals,
      label: t("goals"),
    },
    {
      icon: (
        <ChartNoAxesColumnIncreasingIcon color="var(--base-color-text-primary)" />
      ),
      page: Pages.Trends,
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
      active={props.currentPage === link.page}
      onClick={() => props.setCurrentPage(link.page)}
    />
  ));

  return (
    <Stack justify="space-between" h="100%" p="6px">
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
          icon={<SettingsIcon color="var(--base-color-text-primary)" />}
          label={t("settings")}
          active={props.currentPage === Pages.Settings}
          onClick={() => props.setCurrentPage(Pages.Settings)}
        />
        <NavbarLink
          icon={<LogOutIcon color="var(--base-color-text-primary)" />}
          label={t("logout")}
          onClick={Logout}
        />
      </Stack>
    </Stack>
  );
};

export default Navbar;
