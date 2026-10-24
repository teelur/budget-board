import type { CSSProperties } from "react";
import { budgetBoardColors } from "@teelur/budget-board-ui";

type DarkGhostActionIconStyle = CSSProperties & {
  [key: `--bbui-button-${string}`]: string;
};

const getDarkGhostActionIconStyle = (
  color: string,
): DarkGhostActionIconStyle => ({
  "--bbui-button-bg": "transparent",
  "--bbui-button-color": color,
  "--bbui-button-hover": budgetBoardColors.dark.surfaceHover,
  "--bbui-button-active": `color-mix(in srgb, ${color} 20%, transparent)`,
  "--bbui-button-selected": budgetBoardColors.dark.selection,
  "--bbui-button-selected-color": budgetBoardColors.dark.textPrimary,
  "--bbui-button-border": "transparent",
  "--bbui-button-hover-border": budgetBoardColors.dark.borderStrong,
  "--bbui-button-focus": budgetBoardColors.dark.focusRing,
});

export const darkContrastGhostActionIconStyle =
  getDarkGhostActionIconStyle(budgetBoardColors.dark.contrast);

export const darkTextPrimaryGhostActionIconStyle =
  getDarkGhostActionIconStyle(budgetBoardColors.dark.textPrimary);
