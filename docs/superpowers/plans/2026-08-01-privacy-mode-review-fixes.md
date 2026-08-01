# Privacy Mode Review Fixes Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Address all actionable PR #985 review comments by centralizing sensitive amount formatting, fixing privacy button labels, and preventing status-color privacy leaks.

**Architecture:** Add one pure sensitive-amount formatter and one hook that supplies locale, currency, and privacy state. `SensitiveAmount` becomes the JSX wrapper around that formatter; charts, `Trans` values, and template strings call the hook directly. `StatusText` reads privacy state and falls back to a neutral color when privacy mode is enabled.

**Tech Stack:** React, TypeScript, Mantine, react-i18next, Vite SSR validation script, Yarn.

---

## File Structure

- Modify: `client/src/helpers/privacy.ts`
  - Owns privacy storage constants and the pure sensitive amount formatter.
- Modify: `client/src/components/core/Text/SensitiveAmount/SensitiveAmount.tsx`
  - Exports `useSensitiveAmountFormatter` and renders formatted sensitive amount text for JSX. Both APIs accept an optional currency override for account-specific balances.
- Modify: `client/src/components/core/Text/StatusText/StatusText.tsx`
  - Neutralizes status color while privacy mode is enabled.
- Modify: `client/src/app/Authorized/Header/PrivacyModeButton/PrivacyModeButton.tsx`
  - Shows current visible/hidden state and uses existing i18n source strings.
- Modify: `client/public/locales/en-us/translation.json`
  - Adds English source strings for Weblate.
- Modify: string-only amount call sites:
  - `client/src/components/Charts/MonthlySpendingChart/MonthlySpendingChart.tsx`
  - `client/src/components/Charts/SpendingCategoriesChart/SpendingCategoriesChart.tsx`
  - `client/src/components/Charts/SpendingChart/SpendingChart.tsx`
  - `client/src/components/Charts/ValueChart/ValueChart.tsx`
  - `client/src/components/Charts/NetWorthChart/NetWorthChart.tsx`
  - `client/src/components/Charts/NetCashFlowChart/NetCashFlowChart.tsx`
  - `client/src/components/ui/widgets/MetricWidget/MetricWidget.tsx`
  - `client/src/components/ui/widgets/SpendingTrendsWidget/SpendingTrendsWidget.tsx`
  - `client/src/app/Authorized/PageContent/Goals/GoalCard/GoalCardContent/GoalCardContent.tsx`
  - `client/src/app/Authorized/PageContent/Goals/GoalCard/EditableGoalCardContent/EditableGoalCardContent.tsx`
  - `client/src/app/Authorized/PageContent/Goals/CompletedGoalsAccordion/CompletedGoalCard/CompletedGoalCard.tsx`
  - `client/src/app/Authorized/PageContent/Budgets/BudgetsContent/BudgetSummaryCard/BudgetSummaryItem/BudgetSummaryItem.tsx`
  - `client/src/app/Authorized/PageContent/Budgets/BudgetsContent/BudgetsGroup/BudgetParentCard/BudgetParentCard.tsx`
  - `client/src/app/Authorized/PageContent/Budgets/BudgetsContent/BudgetsGroup/BudgetParentCard/BudgetChildCard/BudgetChildCard.tsx`
  - `client/src/app/Authorized/PageContent/Assets/AssetsContent/AssetDetails/AssetDetails.tsx`
  - `client/src/app/Authorized/PageContent/Assets/AssetsContent/AssetItem/AssetItemContent/AssetItemContent.tsx`
- Modify: JSX amount call sites that still import `maskedAmountText` directly:
  - `client/src/app/Authorized/PageContent/Accounts/AccountsContent/AccountDetails/BalanceItems/BalanceItem/BalanceItemContent/BalanceItemContent.tsx`
  - `client/src/app/Authorized/PageContent/Assets/AssetsContent/AssetDetails/ValueItems/ValueItem/ValueItemContent/ValueItemContent.tsx`
  - `client/src/app/Authorized/PageContent/ExternalAccounts/ExternalAccountsContent/SimpleFinAccountsContent/SimpleFinOrganizationCards/SimpleFinOrganizationCard/SimpleFinAccountCard/SimpleFinAccountCard.tsx`
  - `client/src/app/Authorized/PageContent/ExternalAccounts/ExternalAccountsContent/LunchFlowAccountsContent/LunchFlowInstitutionCards/LunchFlowInstitutionCard/LunchFlowAccountCard/LunchFlowAccountCard.tsx`
- Modify: `client/scripts/validate-privacy-mode.mjs`
  - Adds regression checks for the shared formatter, button labels, locale keys, `StatusText`, and duplicate ternaries.

## Task 1: Add Failing Validation Coverage

**Files:**
- Modify: `client/scripts/validate-privacy-mode.mjs`

- [ ] **Step 1: Add assertions for the new review contracts**

Insert these helpers after `assertOrder`:

```js
function assertNotIncludes(source, unexpected, filePath) {
  assert.ok(
    !source.includes(unexpected),
    `${filePath} must not include ${unexpected}`,
  );
}

function assertJsonKey(source, key, filePath) {
  assertIncludes(source, `"${key}"`, filePath);
}
```

Replace the existing `sensitive amount masks before formatting currency values` test with:

```js
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
  assertIncludes(componentSource, "currency ?? preferredCurrency", "SensitiveAmount.tsx");
});
```

Add this test after the header test:

```js
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
```

Add this test before the chart formatter test:

```js
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
```

Replace the existing `currency chart formatters use the privacy mask` test with:

```js
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
```

Add this test after the chart formatter test:

```js
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
```

- [ ] **Step 2: Run the validation script and confirm it fails**

Run:

```bash
cd client
yarn test:privacy-mode
```

Expected: FAIL. The first failures should mention missing `formatSensitiveAmount`, `sensitive_values_hidden`, `sensitive_values_visible`, `useSensitiveAmountFormatter`, or `usePrivacyMode` in `StatusText`.

- [ ] **Step 3: Commit the failing tests**

```bash
git add client/scripts/validate-privacy-mode.mjs
git commit -m "test: Cover privacy mode review fixes" \
  -m "- Add validation for shared sensitive amount formatting" \
  -m "- Add validation for privacy button labels and status color masking"
```

## Task 2: Add Shared Sensitive Amount Formatting

**Files:**
- Modify: `client/src/helpers/privacy.ts`
- Modify: `client/src/components/core/Text/SensitiveAmount/SensitiveAmount.tsx`

- [ ] **Step 1: Add the pure formatter**

Update `client/src/helpers/privacy.ts` to include the currency imports and formatter:

```ts
import { convertNumberToCurrency, SignDisplay } from "./currency";

export const privacyModeStorageKey = "budgetBoardPrivacyMode";
export const maskedAmountText = "••••";

export const formatSensitiveAmount = (
  amount: number,
  includeCents: boolean,
  currency: string,
  signDisplay: SignDisplay,
  locale: string,
  isPrivacyModeEnabled: boolean,
): string => {
  if (isPrivacyModeEnabled) {
    return maskedAmountText;
  }

  return convertNumberToCurrency(
    amount,
    includeCents,
    currency,
    signDisplay,
    locale,
  );
};
```

Keep the existing `getStoredPrivacyMode` and `storePrivacyMode` functions below this block unchanged.

- [ ] **Step 2: Add the hook and update `SensitiveAmount`**

Replace `client/src/components/core/Text/SensitiveAmount/SensitiveAmount.tsx` with:

```tsx
import React from "react";
import { SignDisplay } from "~/helpers/currency";
import { formatSensitiveAmount } from "~/helpers/privacy";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface SensitiveAmountProps {
  amount: number;
  includeCents?: boolean;
  currency?: string;
  signDisplay?: SignDisplay;
}

export const useSensitiveAmountFormatter = (): ((
  amount: number,
  includeCents?: boolean,
  signDisplay?: SignDisplay,
  currency?: string,
) => string) => {
  const { intlLocale } = useLocale();
  const { isPrivacyModeEnabled } = usePrivacyMode();
  const { preferredCurrency } = useUserSettings();

  return React.useCallback(
    (
      amount: number,
      includeCents = true,
      signDisplay = SignDisplay.Auto,
      currency?: string,
    ): string =>
      formatSensitiveAmount(
        amount,
        includeCents,
        currency ?? preferredCurrency,
        signDisplay,
        intlLocale,
        isPrivacyModeEnabled,
      ),
    [intlLocale, isPrivacyModeEnabled, preferredCurrency],
  );
};

const SensitiveAmount = ({
  amount,
  includeCents = true,
  currency,
  signDisplay = SignDisplay.Auto,
}: SensitiveAmountProps): React.ReactNode => {
  const formatAmount = useSensitiveAmountFormatter();

  return formatAmount(amount, includeCents, signDisplay, currency);
};

export default SensitiveAmount;
```

- [ ] **Step 3: Run the focused validation**

Run:

```bash
cd client
yarn test:privacy-mode
```

Expected: Still FAIL because call sites, button labels, and `StatusText` are not updated yet. The shared formatter test should now pass.

- [ ] **Step 4: Commit the shared formatting API**

```bash
git add client/src/helpers/privacy.ts client/src/components/core/Text/SensitiveAmount/SensitiveAmount.tsx
git commit -m "refactor: Share sensitive amount formatting" \
  -m "- Add a pure privacy-aware currency formatter" \
  -m "- Route SensitiveAmount through the shared formatter hook"
```

## Task 3: Fix Privacy Button State and English Source Strings

**Files:**
- Modify: `client/src/app/Authorized/Header/PrivacyModeButton/PrivacyModeButton.tsx`
- Modify: `client/public/locales/en-us/translation.json`

- [ ] **Step 1: Update the button labels and icon mapping**

Replace the label and icon block in `PrivacyModeButton.tsx` with:

```tsx
  const labelKey = isPrivacyModeEnabled
    ? "sensitive_values_hidden"
    : "sensitive_values_visible";
  const label = t(labelKey);
  const Icon = isPrivacyModeEnabled ? EyeOffIcon : EyeIcon;
```

Keep the `Tooltip`, `ActionIcon`, `aria-label`, and `onClick={togglePrivacyMode}` wiring unchanged.

- [ ] **Step 2: Add English source strings**

In `client/public/locales/en-us/translation.json`, insert these keys in alphabetical order near the other `s...` keys:

```json
  "sensitive_values_hidden": "Sensitive values hidden",
  "sensitive_values_visible": "Sensitive values visible",
```

Do not edit non-English locale files.

- [ ] **Step 3: Run the focused validation**

Run:

```bash
cd client
yarn test:privacy-mode
```

Expected: Still FAIL because `StatusText` and call sites are not updated yet. The privacy button label test should pass.

- [ ] **Step 4: Commit the button and locale change**

```bash
git add client/src/app/Authorized/Header/PrivacyModeButton/PrivacyModeButton.tsx client/public/locales/en-us/translation.json
git commit -m "fix: Show privacy mode state in header" \
  -m "- Use current-state labels and icons for the privacy toggle" \
  -m "- Add English source strings for Weblate"
```

## Task 4: Prevent Status Color Leaks

**Files:**
- Modify: `client/src/components/core/Text/StatusText/StatusText.tsx`

- [ ] **Step 1: Make `StatusText` privacy-aware**

Replace `StatusText.tsx` with:

```tsx
import React from "react";
import { Text, TextProps } from "@mantine/core";
import {
  StatusColorType as StatusColorType,
  getStatusColor,
} from "~/helpers/budgets";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";

interface StatusTextProps extends TextProps {
  amount: number;
  total?: number;
  type?: StatusColorType;
  warningThreshold?: number;
  children?: React.ReactNode;
}

const StatusText = ({
  amount,
  total,
  type,
  warningThreshold,
  children,
  ...props
}: StatusTextProps): React.ReactNode => {
  const { isPrivacyModeEnabled } = usePrivacyMode();
  const color = isPrivacyModeEnabled
    ? "var(--base-color-text-primary)"
    : getStatusColor(
        amount,
        total ?? 0,
        type ?? StatusColorType.Total,
        warningThreshold ?? 110,
      );

  return (
    <Text c={color} fw={props.fw ?? 600} {...props}>
      {children}
    </Text>
  );
};

export default StatusText;
```

- [ ] **Step 2: Run the focused validation**

Run:

```bash
cd client
yarn test:privacy-mode
```

Expected: Still FAIL because chart and interpolation call sites are not updated yet. The `StatusText` privacy test should pass.

- [ ] **Step 3: Commit the status leak fix**

```bash
git add client/src/components/core/Text/StatusText/StatusText.tsx
git commit -m "fix: Hide status color in privacy mode" \
  -m "- Render neutral status text while amounts are masked" \
  -m "- Preserve existing status colors when privacy mode is disabled"
```

## Task 5: Refactor String-Only Amount Call Sites

**Files:**
- Modify all string-only call sites listed in the File Structure section.

- [ ] **Step 1: Replace chart formatter dependencies**

In each chart file, remove direct imports of `convertNumberToCurrency`, `SignDisplay`, `maskedAmountText`, and `usePrivacyMode` when those imports are used only for privacy formatting. Add:

```tsx
import { SignDisplay } from "~/helpers/currency";
import { useSensitiveAmountFormatter } from "~/components/core/Text/SensitiveAmount/SensitiveAmount";
```

Inside each component, add:

```tsx
  const formatSensitiveAmount = useSensitiveAmountFormatter();
```

Use this formatter in chart value functions:

```tsx
  const chartValueFormatter = (value: number | null | undefined): string => {
    if (value == null) {
      return "";
    }

    return formatSensitiveAmount(value, false, SignDisplay.Auto);
  };
```

For one-line chart formatters, use:

```tsx
  const formatValue = (value: number): string =>
    formatSensitiveAmount(value, true, SignDisplay.Auto);
```

- [ ] **Step 2: Replace local interpolation formatter functions**

In each `Trans` or i18n interpolation file, remove direct imports of `convertNumberToCurrency`, `maskedAmountText`, `useLocale` values used only for currency, `useUserSettings` values used only for currency, and `usePrivacyMode` when they exist only for local masking. Add:

```tsx
import { SignDisplay } from "~/helpers/currency";
import { useSensitiveAmountFormatter } from "~/components/core/Text/SensitiveAmount/SensitiveAmount";
```

Inside each component, add:

```tsx
  const formatSensitiveAmount = useSensitiveAmountFormatter();
```

Replace local formatters shaped like this:

```tsx
  const formatSensitiveAmount = (amount: number): string =>
    isPrivacyModeEnabled
      ? maskedAmountText
      : convertNumberToCurrency(
          amount,
          false,
          preferredCurrency,
          SignDisplay.Auto,
          intlLocale,
        );
```

with direct calls:

```tsx
  const formattedAmount = formatSensitiveAmount(amount, false, SignDisplay.Auto);
```

For purchase prices or balances that previously passed `true` for cents, call:

```tsx
  const formattedPrice = formatSensitiveAmount(price, true, SignDisplay.Auto);
```

For values that previously used `SignDisplay.Never`, preserve it:

```tsx
  const formattedProfit = formatSensitiveAmount(
    profit,
    true,
    SignDisplay.Never,
  );
```

- [ ] **Step 3: Run grep checks for direct duplicate masking**

Run:

```bash
rg -n "maskedAmountText|isPrivacyModeEnabled\\s*\\?|convertNumberToCurrency\\(" client/src/components client/src/app/Authorized/PageContent client/src/components/ui/widgets
```

Expected: `maskedAmountText` should remain only in `client/src/helpers/privacy.ts` and validation script references. `convertNumberToCurrency(` may remain where currency formatting is unrelated to privacy mode. No remaining `isPrivacyModeEnabled ? maskedAmountText : convertNumberToCurrency(...)` blocks should appear in PR-changed call sites.

- [ ] **Step 4: Run the focused validation**

Run:

```bash
cd client
yarn test:privacy-mode
```

Expected: PASS for all validation tests.

- [ ] **Step 5: Commit the call-site refactor**

```bash
git add \
  client/src/components/Charts/MonthlySpendingChart/MonthlySpendingChart.tsx \
  client/src/components/Charts/SpendingCategoriesChart/SpendingCategoriesChart.tsx \
  client/src/components/Charts/SpendingChart/SpendingChart.tsx \
  client/src/components/Charts/ValueChart/ValueChart.tsx \
  client/src/components/Charts/NetWorthChart/NetWorthChart.tsx \
  client/src/components/Charts/NetCashFlowChart/NetCashFlowChart.tsx \
  client/src/components/ui/widgets/MetricWidget/MetricWidget.tsx \
  client/src/components/ui/widgets/SpendingTrendsWidget/SpendingTrendsWidget.tsx \
  client/src/app/Authorized/PageContent/Goals/GoalCard/GoalCardContent/GoalCardContent.tsx \
  client/src/app/Authorized/PageContent/Goals/GoalCard/EditableGoalCardContent/EditableGoalCardContent.tsx \
  client/src/app/Authorized/PageContent/Goals/CompletedGoalsAccordion/CompletedGoalCard/CompletedGoalCard.tsx \
  client/src/app/Authorized/PageContent/Budgets/BudgetsContent/BudgetSummaryCard/BudgetSummaryItem/BudgetSummaryItem.tsx \
  client/src/app/Authorized/PageContent/Budgets/BudgetsContent/BudgetsGroup/BudgetParentCard/BudgetParentCard.tsx \
  client/src/app/Authorized/PageContent/Budgets/BudgetsContent/BudgetsGroup/BudgetParentCard/BudgetChildCard/BudgetChildCard.tsx \
  client/src/app/Authorized/PageContent/Assets/AssetsContent/AssetDetails/AssetDetails.tsx \
  client/src/app/Authorized/PageContent/Assets/AssetsContent/AssetItem/AssetItemContent/AssetItemContent.tsx
git commit -m "refactor: Reuse sensitive amount formatter" \
  -m "- Replace local privacy masking ternaries in charts and interpolated strings" \
  -m "- Keep string-only call sites on the same formatter as SensitiveAmount"
```

## Task 6: Convert Remaining JSX Amount Displays

**Files:**
- Modify the JSX amount files listed in the File Structure section.

- [ ] **Step 1: Replace direct JSX ternaries with `SensitiveAmount`**

For each `StatusText` child that renders:

```tsx
{isPrivacyModeEnabled
  ? maskedAmountText
  : convertNumberToCurrency(amount, true, currency, SignDisplay.Auto, intlLocale)}
```

replace it with:

```tsx
<SensitiveAmount amount={amount} />
```

If the component uses an account-specific currency instead of the preferred user currency, pass that currency into `SensitiveAmount`:

```tsx
<SensitiveAmount amount={amount} currency={accountCurrency} />
```

For string-only account-currency displays, pass the currency override to the shared formatter:

```tsx
formatSensitiveAmount(amount, true, SignDisplay.Auto, accountCurrency)
```

- [ ] **Step 2: Run the focused validation and TypeScript build**

Run:

```bash
cd client
yarn test:privacy-mode
yarn build
```

Expected: both commands PASS.

- [ ] **Step 3: Commit JSX cleanup**

```bash
git add \
  client/src/app/Authorized/PageContent/Accounts/AccountsContent/AccountDetails/BalanceItems/BalanceItem/BalanceItemContent/BalanceItemContent.tsx \
  client/src/app/Authorized/PageContent/Assets/AssetsContent/AssetDetails/ValueItems/ValueItem/ValueItemContent/ValueItemContent.tsx \
  client/src/app/Authorized/PageContent/ExternalAccounts/ExternalAccountsContent/SimpleFinAccountsContent/SimpleFinOrganizationCards/SimpleFinOrganizationCard/SimpleFinAccountCard/SimpleFinAccountCard.tsx \
  client/src/app/Authorized/PageContent/ExternalAccounts/ExternalAccountsContent/LunchFlowAccountsContent/LunchFlowInstitutionCards/LunchFlowInstitutionCard/LunchFlowAccountCard/LunchFlowAccountCard.tsx
git commit -m "refactor: Use SensitiveAmount for jsx amount displays" \
  -m "- Replace duplicate masked amount rendering in JSX amount displays" \
  -m "- Preserve existing formatting semantics for account-specific currency displays"
```

## Task 7: Final Verification and PR Update

**Files:**
- Verify all modified files.

- [ ] **Step 1: Check branch divergence before pushing**

Run:

```bash
git fetch upstream main
git status --short --branch
git rev-list --left-right --count upstream/main...HEAD
```

Expected: working tree clean. The branch may still be behind upstream; do not rewrite unrelated upstream changes unless merge conflicts require it.

- [ ] **Step 2: Run final checks**

Run:

```bash
git diff --check upstream/main...HEAD
cd client
yarn test:privacy-mode
yarn build
```

Expected: all commands PASS.

- [ ] **Step 3: Push the PR branch**

Confirm the PR head is `joao-baza/budget-board:feat/privacy-mode`, then push:

```bash
git push origin feat/privacy-mode
```

Expected: push succeeds and PR #985 receives the new commits.

- [ ] **Step 4: Summarize addressed comments**

Report:

- English source strings restored for Weblate.
- Privacy button now shows current visible/hidden state.
- Sensitive amount formatting is centralized for JSX and string call sites.
- `StatusText` no longer leaks sign/status color in privacy mode.
- Validation commands and final push result.
