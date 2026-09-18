import { defineConfig } from "vite";
import react, { reactCompilerPreset } from "@vitejs/plugin-react";
import babel from "@rolldown/plugin-babel";
import path from "path";
import { fileURLToPath } from "url";

const configDirectory = path.dirname(fileURLToPath(import.meta.url));

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), babel({ presets: [reactCompilerPreset()] })],
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
