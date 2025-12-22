import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";
// https://vite.dev/config/
export default defineConfig({
    plugins: [
        react({
            babel: {
                plugins: [["babel-plugin-react-compiler", { target: "19" }]],
            },
        }),
    ],
    build: {
        rollupOptions: {
            output: {
                format: "es",
                globals: {
                    react: "React",
                    "react-dom": "ReactDOM",
                },
                manualChunks: function (id) {
                    if (/projectEnvVariables.ts/.test(id)) {
                        return "projectEnvVariables";
                    }
                },
            },
        },
    },
    resolve: {
        alias: {
            "~": path.resolve(__dirname, "./src"),
        },
    },
});
