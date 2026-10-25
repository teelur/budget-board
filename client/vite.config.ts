import { defineConfig } from "vite";
import react, { reactCompilerPreset } from "@vitejs/plugin-react";
import babel from "@rolldown/plugin-babel";
import { budgetBoardColors } from "@teelur/budget-board-ui";
import path from "path";
import { fileURLToPath } from "url";

const configDirectory = path.dirname(fileURLToPath(import.meta.url));
const budgetBoardThemeColorsPlugin = {
  name: "budget-board-theme-colors",
  transformIndexHtml(html: string) {
    return html
      .replaceAll("__BBUI_PAGE_LIGHT__", budgetBoardColors.light.page)
      .replaceAll("__BBUI_PAGE_DARK__", budgetBoardColors.dark.page)
      .replaceAll("__BBUI_BORDER_LIGHT__", budgetBoardColors.light.border)
      .replaceAll("__BBUI_BORDER_DARK__", budgetBoardColors.dark.border)
      .replaceAll("__BBUI_PRIMARY_LIGHT__", budgetBoardColors.light.primary)
      .replaceAll("__BBUI_PRIMARY_DARK__", budgetBoardColors.dark.primary);
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
