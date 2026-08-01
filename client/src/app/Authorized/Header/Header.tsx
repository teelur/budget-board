import BudgetBoardLogo from "~/assets/budget-board-logo";
import classes from "./Header.module.css";

import { Box, Burger, Group, useComputedColorScheme } from "@mantine/core";
import { useMediaQuery } from "@mantine/hooks";
import PrivacyModeButton from "./PrivacyModeButton/PrivacyModeButton";
import SyncButton from "./SyncButton/SyncButton";
import { areStringsEqual } from "~/helpers/utils";
import { useNavigate } from "react-router";

interface HeaderProps {
  isNavbarOpen: boolean;
  toggleNavbar: () => void;
}

const Header = (props: HeaderProps): React.ReactNode => {
  const computedColorScheme = useComputedColorScheme();
  const isCompactHeader = useMediaQuery("(max-width: 30em)", false, {
    getInitialValueInEffect: false,
  });
  const navigate = useNavigate();

  return (
    <Group
      p="0.5rem"
      h="100%"
      justify="space-between"
      align="center"
      wrap="nowrap"
    >
      <Group gap="0.5rem" wrap="nowrap" style={{ minWidth: 0 }}>
        <Burger
          opened={props.isNavbarOpen}
          className={classes.burger}
          onClick={props.toggleNavbar}
          hiddenFrom="xs"
          size="md"
        />
        <Box
          onClick={() => navigate("/dashboard")}
          style={{ cursor: "pointer", lineHeight: 0 }}
        >
          <BudgetBoardLogo
            height={isCompactHeader ? 22 : 40}
            darkMode={areStringsEqual(computedColorScheme, "dark")}
          />
        </Box>
      </Group>
      <Group justify="flex-end" flex="0 0 auto" gap="xs" wrap="nowrap">
        <PrivacyModeButton />
        <SyncButton compact={isCompactHeader} />
      </Group>
    </Group>
  );
};

export default Header;
