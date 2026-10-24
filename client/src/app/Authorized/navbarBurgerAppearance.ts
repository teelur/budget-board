import { budgetBoardColors } from "@teelur/budget-board-ui";

export const getNavbarBurgerColor = (
  isNavbarOpen: boolean,
  isDarkMode: boolean,
) =>
  isNavbarOpen
    ? "#ffffff"
    : isDarkMode
      ? budgetBoardColors.dark.textPrimary
      : budgetBoardColors.light.textPrimary;

export const openNavbarBurgerStyles = {
  burger: {
    filter: `drop-shadow(0 0 1px ${budgetBoardColors.dark.textInverse})`,
  },
};
