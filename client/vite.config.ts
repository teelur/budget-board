import { defineConfig } from "vite";
import react, { reactCompilerPreset } from "@vitejs/plugin-react";
import babel from "@rolldown/plugin-babel";
import { budgetBoardColors } from "@teelur/budget-board-ui";
import path from "path";
import { fileURLToPath } from "url";

const configDirectory = path.dirname(fileURLToPath(import.meta.url));
const budgetBoardPageColorsPlugin = {
  name: "budget-board-page-colors",
  transformIndexHtml(html: string) {
    return html
      .replaceAll("__BBUI_PAGE_LIGHT__", budgetBoardColors.light.page)
      .replaceAll("__BBUI_PAGE_DARK__", budgetBoardColors.dark.page);
  },
};

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    budgetBoardPageColorsPlugin,
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
