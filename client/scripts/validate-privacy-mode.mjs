import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { createServer } from "vite";

const rootDir = path.resolve(
  path.dirname(fileURLToPath(import.meta.url)),
  "..",
);

const tests = [];

function test(name, fn) {
  tests.push({ name, fn });
}

function fromRoot(...parts) {
  return path.join(rootDir, ...parts);
}

async function readSource(relativePath) {
  return readFile(fromRoot(relativePath), "utf8");
}

function assertIncludes(source, expected, filePath) {
  assert.ok(source.includes(expected), `${filePath} must include ${expected}`);
}

function assertOrder(source, expectedOrder, filePath) {
  let lastIndex = -1;

  expectedOrder.forEach((expected) => {
    const index = source.indexOf(expected);
    assert.notEqual(index, -1, `${filePath} must include ${expected}`);
    assert.ok(
      index > lastIndex,
      `${filePath} must keep ${expected} after the previous provider`,
    );
    lastIndex = index;
  });
}

function assertNotIncludes(source, unexpected, filePath) {
  assert.ok(
    !source.includes(unexpected),
    `${filePath} must not include ${unexpected}`,
  );
}

function assertJsonKey(source, key, filePath) {
  assertIncludes(source, `"${key}"`, filePath);
}

const vite = await createServer({
  root: rootDir,
  configFile: fromRoot("vite.config.ts"),
  logLevel: "error",
  server: { middlewareMode: true },
  appType: "custom",
});

test("privacy mode storage is resilient and persists boolean values", async () => {
  const { getStoredPrivacyMode, privacyModeStorageKey, storePrivacyMode } =
    await vite.ssrLoadModule("/src/helpers/privacy.ts");
  const originalWindow = globalThis.window;

  try {
    Reflect.deleteProperty(globalThis, "window");
    assert.equal(getStoredPrivacyMode(), false);
    assert.doesNotThrow(() => storePrivacyMode(true));

    const writes = [];
    globalThis.window = {
      localStorage: {
        getItem(key) {
          assert.equal(key, privacyModeStorageKey);
          return "true";
        },
        setItem(key, value) {
          writes.push([key, value]);
        },
      },
    };

    assert.equal(getStoredPrivacyMode(), true);
    storePrivacyMode(false);
    storePrivacyMode(true);
    assert.deepEqual(writes, [
      [privacyModeStorageKey, "false"],
      [privacyModeStorageKey, "true"],
    ]);

    globalThis.window = {
      localStorage: {
        getItem() {
          throw new Error("localStorage read blocked");
        },
        setItem() {
          throw new Error("localStorage write blocked");
        },
      },
    };

    assert.equal(getStoredPrivacyMode(), false);
    assert.doesNotThrow(() => storePrivacyMode(true));
  } finally {
    if (originalWindow === undefined) {
      Reflect.deleteProperty(globalThis, "window");
    } else {
      globalThis.window = originalWindow;
    }
  }
});

test("metric templates report whether they contain currency metrics", async () => {
  const { hasCurrencyMetric, parseTemplate } = await vite.ssrLoadModule(
    "/src/helpers/metricWidget.ts",
  );

  assert.equal(
    hasCurrencyMetric(parseTemplate("@transactions.sum(this_month)")),
    true,
  );
  assert.equal(
    hasCurrencyMetric(parseTemplate("@accounts.balance(all_time)")),
    true,
  );
  assert.equal(
    hasCurrencyMetric(parseTemplate("@budgets.percent_used(this_month)")),
    false,
  );
  assert.equal(
    hasCurrencyMetric(parseTemplate("@transactions.count(this_month)")),
    false,
  );
  assert.equal(hasCurrencyMetric(parseTemplate("static label")), false);
  assert.equal(
    hasCurrencyMetric(
      parseTemplate(
        "saved @goals.current_amount(all_time,name=Emergency) / @goals.percent_complete(all_time,name=Emergency)",
      ),
    ),
    true,
  );
});

test("privacy mode provider wraps the authorized app tree", async () => {
  const source = await readSource("src/App.tsx");

  assertIncludes(
    source,
    "PrivacyModeProvider",
    "src/App.tsx",
  );
  assertOrder(
    source,
    ["<UserSettingsProvider>", "<LocaleProvider>", "<PrivacyModeProvider>"],
    "src/App.tsx",
  );
});

test("metric widget uses shared formatting for currency templates", async () => {
  const source = await readSource(
    "src/components/ui/widgets/MetricWidget/MetricWidget.tsx",
  );

  assertIncludes(source, "useSensitiveAmountFormatter", "MetricWidget.tsx");
  assertIncludes(
    source,
    "hasCurrencyMetric(parsedValueTokens)",
    "MetricWidget.tsx",
  );
  assertIncludes(
    source,
    "hasCurrencyMetric(parsedLabelTokens)",
    "MetricWidget.tsx",
  );
  assertIncludes(source, "formatSensitiveAmount", "MetricWidget.tsx");
  assertNotIncludes(source, "return maskedAmountText;", "MetricWidget.tsx");
});

test("sensitive amount formatting is shared by string and jsx callers", async () => {
  const helperSource = await readSource("src/helpers/privacy.ts");
  const componentSource = await readSource(
    "src/components/core/Text/SensitiveAmount/SensitiveAmount.tsx",
  );

  assertIncludes(helperSource, "formatSensitiveAmount", "privacy.ts");
  assertIncludes(helperSource, "isPrivacyModeEnabled", "privacy.ts");
  assertIncludes(helperSource, "return maskedAmountText;", "privacy.ts");
  assertIncludes(helperSource, "convertNumberToCurrency", "privacy.ts");
  assertIncludes(
    componentSource,
    "useSensitiveAmountFormatter",
    "SensitiveAmount.tsx",
  );
  assertIncludes(
    componentSource,
    "return formatAmount(amount, includeCents, signDisplay, currency);",
    "SensitiveAmount.tsx",
  );
  assertIncludes(
    componentSource,
    "currency ?? preferredCurrency",
    "SensitiveAmount.tsx",
  );
});

test("privacy button and compact sync controls remain reachable in the header", async () => {
  const header = await readSource("src/app/Authorized/Header/Header.tsx");
  const syncButton = await readSource(
    "src/app/Authorized/Header/SyncButton/SyncButton.tsx",
  );

  assertIncludes(header, "<PrivacyModeButton />", "Header.tsx");
  assertIncludes(
    header,
    '<SyncButton compact={isCompactHeader} />',
    "Header.tsx",
  );
  assertIncludes(header, 'useMediaQuery("(max-width: 30em)"', "Header.tsx");
  assertIncludes(header, "getInitialValueInEffect: false", "Header.tsx");
  assertIncludes(syncButton, "compact?: boolean", "SyncButton.tsx");
  assertIncludes(syncButton, "<ActionIcon", "SyncButton.tsx");
  assertIncludes(syncButton, "aria-label={syncLabel}", "SyncButton.tsx");
});

test("privacy button uses current-state labels from English source strings", async () => {
  const button = await readSource(
    "src/app/Authorized/Header/PrivacyModeButton/PrivacyModeButton.tsx",
  );
  const enUs = await readSource("public/locales/en-us/translation.json");

  assertIncludes(button, "sensitive_values_hidden", "PrivacyModeButton.tsx");
  assertIncludes(button, "sensitive_values_visible", "PrivacyModeButton.tsx");
  assertNotIncludes(button, "show_sensitive_values", "PrivacyModeButton.tsx");
  assertNotIncludes(button, "hide_sensitive_values", "PrivacyModeButton.tsx");
  assertJsonKey(enUs, "sensitive_values_hidden", "en-us/translation.json");
  assertJsonKey(enUs, "sensitive_values_visible", "en-us/translation.json");
});

test("status text hides status color while privacy mode is enabled", async () => {
  const source = await readSource(
    "src/components/core/Text/StatusText/StatusText.tsx",
  );

  assertIncludes(source, "usePrivacyMode", "StatusText.tsx");
  assertIncludes(source, "isPrivacyModeEnabled", "StatusText.tsx");
  assertIncludes(
    source,
    "var(--base-color-text-primary)",
    "StatusText.tsx",
  );
  assertOrder(
    source,
    ["isPrivacyModeEnabled", "getStatusColor"],
    "StatusText.tsx",
  );
});

test("currency chart formatters use the shared sensitive amount formatter", async () => {
  const chartFiles = [
    "src/components/Charts/MonthlySpendingChart/MonthlySpendingChart.tsx",
    "src/components/Charts/NetCashFlowChart/NetCashFlowChart.tsx",
    "src/components/Charts/NetWorthChart/NetWorthChart.tsx",
    "src/components/Charts/SpendingCategoriesChart/SpendingCategoriesChart.tsx",
    "src/components/Charts/SpendingChart/SpendingChart.tsx",
    "src/components/Charts/ValueChart/ValueChart.tsx",
  ];

  for (const filePath of chartFiles) {
    const source = await readSource(filePath);
    assertIncludes(source, "useSensitiveAmountFormatter", filePath);
    assertIncludes(source, "formatSensitiveAmount", filePath);
    assertNotIncludes(source, "isPrivacyModeEnabled", filePath);
    assertNotIncludes(source, "maskedAmountText", filePath);
  }
});

test("string interpolation callers use the shared sensitive amount formatter", async () => {
  const stringCallers = [
    "src/components/ui/widgets/SpendingTrendsWidget/SpendingTrendsWidget.tsx",
    "src/app/Authorized/PageContent/Goals/GoalCard/GoalCardContent/GoalCardContent.tsx",
    "src/app/Authorized/PageContent/Goals/GoalCard/EditableGoalCardContent/EditableGoalCardContent.tsx",
    "src/app/Authorized/PageContent/Goals/CompletedGoalsAccordion/CompletedGoalCard/CompletedGoalCard.tsx",
    "src/app/Authorized/PageContent/Budgets/BudgetsContent/BudgetSummaryCard/BudgetSummaryItem/BudgetSummaryItem.tsx",
    "src/app/Authorized/PageContent/Budgets/BudgetsContent/BudgetsGroup/BudgetParentCard/BudgetParentCard.tsx",
    "src/app/Authorized/PageContent/Budgets/BudgetsContent/BudgetsGroup/BudgetParentCard/BudgetChildCard/BudgetChildCard.tsx",
    "src/app/Authorized/PageContent/Assets/AssetsContent/AssetDetails/AssetDetails.tsx",
  ];

  for (const filePath of stringCallers) {
    const source = await readSource(filePath);
    assertIncludes(source, "useSensitiveAmountFormatter", filePath);
    assertNotIncludes(source, "maskedAmountText", filePath);
  }
});

try {
  for (const { name, fn } of tests) {
    await fn();
    console.log(`ok - ${name}`);
  }
} finally {
  await vite.close();
}
