# Privacy Mode Review Fixes Design

## Context

PR #985 adds a client-side privacy mode to Budget Board. The feature stores a browser-level toggle in local storage, adds a header button, and masks monetary values across cards, widgets, and charts.

The PR has requested changes. The review feedback asks for less duplicated hidden-versus-visible amount logic, better reuse of `SensitiveAmount`, missing English translation keys, an inverted privacy button state, and protection against leaking value sign through `StatusText` color.

## Goals

- Address every unresolved actionable review thread on PR #985.
- Keep privacy masking behavior in one shared formatting path.
- Support both `ReactNode` call sites and string-only call sites such as chart formatters and i18n interpolation.
- Add only English source strings for the new privacy button labels so Weblate can translate the rest.
- Stop visual color from revealing positive, negative, warning, or target status when privacy mode hides the amount.
- Preserve the existing PR scope: client-side privacy mode only.

## Non-Goals

- Do not add translations to non-English locale files.
- Do not redesign the header beyond the requested privacy button state change.
- Do not change server APIs, persisted user settings, or account data models.
- Do not resolve GitHub review threads or submit a GitHub review unless asked.

## Review Thread Mapping

The implementation will treat the review comments as five clusters.

1. Missing i18n keys: add English keys for the privacy button labels in `client/public/locales/en-us/translation.json`.
2. Button state: make the icon and accessible label represent the current state, not the action that will happen after click.
3. Duplicated amount masking logic: centralize `maskedAmountText` versus `convertNumberToCurrency` decisions in a shared helper path.
4. SensitiveAmount reuse: update direct masking blocks and local `formatSensitiveAmount` functions to use the shared path or `SensitiveAmount`.
5. Status color leak: make privacy-hidden amounts render with a neutral text color instead of a color derived from the real amount.

Outdated chart threads still point to the same design issue. They will be considered addressed if the chart formatter code uses the shared string formatter.

## Proposed Design

### Shared Formatting API

Add a pure formatter that accepts the amount, currency, locale, privacy state, cents setting, and sign display. It returns the masked text when privacy mode is enabled and otherwise delegates to `convertNumberToCurrency`.

Add a hook, tentatively `useSensitiveAmountFormatter`, that reads `intlLocale`, `preferredCurrency`, and `isPrivacyModeEnabled`, then returns a function with the same amount formatting options. Components that need a string can call this hook directly.

Keep `SensitiveAmount` as the component for JSX call sites. It should call the shared hook and return the resulting string. This preserves the readable component API while giving charts and `Trans` values the same logic.

### Status Text Privacy Behavior

Extend `StatusText` with an optional privacy-aware behavior. When privacy mode is active, `StatusText` should render a neutral/base text color instead of calling `getStatusColor` with the real amount. This prevents sign and threshold leaks while preserving layout, weight, and other text props.

The default should protect privacy-enabled displays without requiring every caller to remember a special prop. If a future caller needs status color while privacy mode is enabled, that should be an explicit opt-out, not the default.

### Privacy Button Labels

Change the button to show the current state:

- Privacy enabled: show the hidden-state icon and label.
- Privacy disabled: show the visible-state icon and label.

Use English source keys such as `sensitive_values_hidden` and `sensitive_values_visible`, or reuse the existing key style if the codebase has a closer naming pattern. Add those keys only to `en-us`.

### Call-Site Cleanup

Replace local formatter functions and inline ternaries that duplicate masking logic with the shared formatter. Use `SensitiveAmount` for plain JSX amount displays. Use the string formatter for charts, `Trans` interpolation values, and dynamic strings.

Where a displayed amount currently sits inside `StatusText`, keep `StatusText` as the wrapper and use `SensitiveAmount` or the shared formatter for the child text. The updated `StatusText` behavior will prevent color leaks.

## Data Flow

`PrivacyModeProvider` remains the source of privacy state. Components access it in two ways:

- JSX amount display: `SensitiveAmount` reads privacy state through the shared hook and returns masked or formatted text.
- String amount display: the component calls `useSensitiveAmountFormatter` and passes the result to chart formatters, i18n values, or template renderers.

Both paths use the same pure formatter. Future privacy display states can be added in one place.

## Error Handling

Existing local storage error handling stays in `getStoredPrivacyMode` and `storePrivacyMode`. The formatter path should not throw for privacy mode itself. Currency formatting keeps the existing `convertNumberToCurrency` behavior.

If privacy mode is enabled, the formatter returns the mask before running currency formatting. This avoids formatting edge cases and prevents real values from reaching formatting output.

## Testing

Update `client/scripts/validate-privacy-mode.mjs` to verify:

- The shared pure formatter returns the mask when privacy mode is enabled.
- `SensitiveAmount` delegates to the shared formatting path.
- Chart formatter files and string-only consumers use the shared formatter instead of local ternaries.
- `PrivacyModeButton` uses current-state labels and the English locale file contains those keys.
- `StatusText` neutralizes status color when privacy mode is active.

Run at least:

- `yarn test:privacy-mode`
- `yarn build`
- `git diff --check`

If lint fails for the known ESLint/plugin mismatch seen in earlier Budget Board work, record the exact error and use the targeted privacy-mode script plus build as fallback evidence.

## Risks

- `StatusText` is shared. Making it privacy-aware must preserve current colors when privacy mode is disabled.
- Some review comments ask for `SensitiveAmount`, but some call sites require a string. The shared hook must make that distinction explicit.
- The PR branch is behind `upstream/main`. Rebase or merge should happen only after checking the branch state and avoiding unrelated conflict churn.

## Rollout

Commit the implementation to `feat/privacy-mode` in the fork branch for PR #985. Push only after tests pass or after any test blocker is documented with evidence.
