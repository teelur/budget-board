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

test("metric widget masks currency value and label templates only while private", async () => {
  const source = await readSource(
    "src/components/ui/widgets/MetricWidget/MetricWidget.tsx",
  );

  assertIncludes(source, "usePrivacyMode", "MetricWidget.tsx");
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
  assertIncludes(source, "return maskedAmountText;", "MetricWidget.tsx");
});

test("sensitive amount masks before formatting currency values", async () => {
  const source = await readSource(
    "src/components/core/Text/SensitiveAmount/SensitiveAmount.tsx",
  );

  assertIncludes(source, "usePrivacyMode", "SensitiveAmount.tsx");
  assertIncludes(source, "return maskedAmountText;", "SensitiveAmount.tsx");
  assertIncludes(source, "convertNumberToCurrency", "SensitiveAmount.tsx");
  assertOrder(
    source,
    [
      "if (isPrivacyModeEnabled)",
      "return maskedAmountText;",
      "return convertNumberToCurrency",
    ],
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

test("currency chart formatters use the privacy mask", async () => {
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
    assertIncludes(source, "usePrivacyMode", filePath);
    assertIncludes(source, "isPrivacyModeEnabled", filePath);
    assertIncludes(source, "maskedAmountText", filePath);
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
