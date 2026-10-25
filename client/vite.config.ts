import { defineConfig } from "vite";
import react, { reactCompilerPreset } from "@vitejs/plugin-react";
import babel from "@rolldown/plugin-babel";
import { budgetBoardColors } from "@teelur/budget-board-ui";
import { readFileSync } from "node:fs";
import path from "path";
import { fileURLToPath } from "url";

const configDirectory = path.dirname(fileURLToPath(import.meta.url));
const englishTranslations = JSON.parse(
  readFileSync(
    path.resolve(configDirectory, "public/locales/en-us/translation.json"),
    "utf-8",
  ),
) as { starting_application?: string };
const startupMessage = englishTranslations.starting_application;

if (typeof startupMessage !== "string") {
  throw new Error(
    'Missing "starting_application" in the en-us translation file.',
  );
}

const escapeHtmlAttribute = (value: string): string =>
  value
    .replaceAll("&", "&amp;")
    .replaceAll('"', "&quot;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;");

const budgetBoardThemeColorsPlugin = {
  name: "budget-board-theme-colors",
  transformIndexHtml(html: string) {
    return html
      .replaceAll(
        "__BBUI_STARTUP_MESSAGE__",
        escapeHtmlAttribute(startupMessage),
      )
      .replaceAll("__BBUI_PAGE_LIGHT__", budgetBoardColors.light.page)
      .replaceAll("__BBUI_PAGE_DARK__", budgetBoardColors.dark.page)
      .replaceAll("__BBUI_BORDER_LIGHT__", budgetBoardColors.light.border)
      .replaceAll("__BBUI_BORDER_DARK__", budgetBoardColors.dark.border)
      .replaceAll("__BBUI_PRIMARY_LIGHT__", budgetBoardColors.light.primary)
      .replaceAll("__BBUI_PRIMARY_DARK__", budgetBoardColors.dark.primary)
      .replaceAll(
        "__BBUI_TEXT_PRIMARY_LIGHT__",
        budgetBoardColors.light.textPrimary,
      )
      .replaceAll(
        "__BBUI_TEXT_PRIMARY_DARK__",
        budgetBoardColors.dark.textPrimary,
      );
  },
};

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    budgetBoardThemeColorsPlugin,
    react(),
    babel({ presets: [reactCompilerPreset()] }),
  ],
  build: {
    rollupOptions: {
      output: {
        format: "es",
        globals: {
          react: "React",
          "react-dom": "ReactDOM",
        },
        manualChunks(id) {
          if (/projectEnvVariables.ts/.test(id)) {
            return "projectEnvVariables";
          }
        },
      },
    },
  },
  resolve: {
    alias: {
      "~": path.resolve(configDirectory, "./src"),
    },
  },
});
