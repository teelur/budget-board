import classes from "./Trends.module.css";

import { Box, Group, Stack } from "@mantine/core";
import {
  ArrowRightLeftIcon,
  ChartLineIcon,
  CreditCardIcon,
  GitBranchIcon,
  LandmarkIcon,
  LineChartIcon,
  TagsIcon,
  TrendingUpIcon,
} from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import { Outlet, useLocation, useNavigate } from "react-router";
import { BodyText, NavbarLink } from "@teelur/budget-board-ui";

const Trends = (): React.ReactNode => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();

  const navigateToTrend = (path: string) => {
    navigate(path);
  };

  const navbarLinkSections = [
    {
      label: t("transactions"),
      links: [
        {
          label: t("spending"),
          path: "spending",
          icon: <LineChartIcon color="currentColor" />,
        },
        {
          label: t("spending_categories"),
          path: "spending-categories",
          icon: <TagsIcon color="currentColor" />,
        },
        {
          label: t("net_cash_flow"),
          path: "net-cash-flow",
          icon: <ArrowRightLeftIcon color="currentColor" />,
        },
        {
          label: t("flows"),
          path: "flows",
          icon: <GitBranchIcon color="currentColor" />,
        },
      ],
    },
    {
      label: t("accounts"),
      links: [
        {
          label: t("assets"),
          path: "assets",
          icon: <LandmarkIcon color="currentColor" />,
        },
        {
          label: t("liabilities"),
          path: "liabilities",
          icon: <CreditCardIcon color="currentColor" />,
        },
        {
          label: t("net_worth"),
          path: "net-worth",
          icon: <TrendingUpIcon color="currentColor" />,
        },
      ],
    },
    {
      label: t("assets"),
      links: [
        {
          label: t("values"),
          path: "values",
          icon: <ChartLineIcon color="currentColor" />,
        },
      ],
    },
  ];

  return (
    <Stack className={classes.root} p="0.5rem">
      <Group align="flex-start" gap="md" wrap="wrap">
        <Stack
          w={{ base: "100%", md: "200px" }}
          gap={4}
          style={{
            flexShrink: 0,
          }}
        >
          {navbarLinkSections.map((section, sectionIndex) => (
            <React.Fragment key={section.label}>
              <BodyText
                tone="secondary"
                size="xs"
                px="0.5rem"
                mt={sectionIndex === 0 ? undefined : "xs"}
              >
                {section.label}
              </BodyText>
              {section.links.map((link) => (
                <NavbarLink
                  mode="selection"
                  key={link.path}
                  label={link.label}
                  icon={link.icon}
                  showLabel
                  active={location.pathname.endsWith(`/${link.path}`)}
                  onClick={() => navigateToTrend(link.path)}
                />
              ))}
            </React.Fragment>
          ))}
        </Stack>
        <Box w={{ base: "100%", sm: "auto" }} style={{ flex: 1, minWidth: 0 }}>
          <Outlet />
        </Box>
      </Group>
    </Stack>
  );
};

export default Trends;
