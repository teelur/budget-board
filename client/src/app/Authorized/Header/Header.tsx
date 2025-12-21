import BudgetBoardLogo from "~/assets/budget-board-logo";
import classes from "./Header.module.css";

import { Burger, Group, useComputedColorScheme } from "@mantine/core";
import SyncButton from "./SyncButton/SyncButton";
import { areStringsEqual } from "~/helpers/utils";

interface HeaderProps {
  isNavbarOpen: boolean;
  toggleNavbar: () => void;
}

const Header = (props: HeaderProps): React.ReactNode => {
  const computedColorScheme = useComputedColorScheme();
  return (
    <Group
      p="0.5rem"
      h="100%"
      justify="space-between"
      align="center"
      wrap="nowrap"
    >
      <Group gap="0.5rem" wrap="nowrap">
        <Burger
          opened={props.isNavbarOpen}
          className={classes.burger}
          onClick={props.toggleNavbar}
          hiddenFrom="xs"
          size="md"
        />
        <BudgetBoardLogo
          height={40}
          darkMode={areStringsEqual(computedColorScheme, "dark")}
        />
      </Group>
      <Group justify="flex-end" flex="1 0 auto">
        <SyncButton />
      </Group>
    </Group>
  );
};

export default Header;
