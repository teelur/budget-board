import mantine from "eslint-config-mantine";
import tseslint from "typescript-eslint";
import jsonc from "eslint-plugin-jsonc";

export default [
  ...tseslint.config(...mantine),
  {
    ignores: ["**/*.{mjs,cjs,js,d.ts,d.mts}", "./.storybook/main.ts"],
  },
  ...jsonc.configs["flat/recommended-with-json"],
  {
    files: ["**/locales/**/*.json"],
    rules: {
      "jsonc/sort-keys": [
        "error",
        "asc",
        {
          caseSensitive: true,
          natural: false,
          minKeys: 2,
        },
      ],
    },
  },
];
