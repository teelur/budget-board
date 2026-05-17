import { Box, ScrollArea, Stack } from "@mantine/core";
import { Navigate, Route, Routes } from "react-router";
import { Suspense, lazy } from "react";
import LoadingScreen from "~/components/LoadingScreen/LoadingScreen";

const Dashboard = lazy(() => import("./Dashboard/Dashboard"));
const Accounts = lazy(() => import("./Accounts/Accounts"));
const Assets = lazy(() => import("./Assets/Assets"));
const Transactions = lazy(() => import("./Transactions/Transactions"));
const TransactionsSettings = lazy(
  () => import("./Transactions/TransactionsSettings/TransactionsSettings"),
);
const AccountsSettings = lazy(
  () => import("./Accounts/AccountsSettings/AccountsSettings"),
);
const AccountsSettingsDeleted = lazy(
  () => import("./Accounts/AccountsSettings/DeletedAccounts/DeletedAccounts"),
);
const AccountsSettingsAccountTypes = lazy(
  () => import("./Accounts/AccountsSettings/AccountTypes/AccountTypes"),
);
const AssetsSettings = lazy(
  () => import("./Assets/AssetsSettings/AssetsSettings"),
);
const AssetsSettingsDeleted = lazy(
  () => import("./Assets/AssetsSettings/DeletedAssets/DeletedAssets"),
);
const TransactionsSettingsCategories = lazy(
  () => import("./Transactions/TransactionsSettings/Categories/Categories"),
);
const TransactionsSettingsRules = lazy(
  () =>
    import("./Transactions/TransactionsSettings/AutomaticRules/AutomaticRules"),
);
const TransactionsSettingsDeleted = lazy(
  () =>
    import("./Transactions/TransactionsSettings/DeletedTransactions/DeletedTransactions"),
);
const TransactionsSettingsAutoCategorizer = lazy(
  () =>
    import("./Transactions/TransactionsSettings/AutoCategorizer/AutoCategorizer"),
);
const Budgets = lazy(() => import("./Budgets/Budgets"));
const BudgetsSettings = lazy(
  () => import("./Budgets/BudgetsSettings/BudgetsSettings"),
);
const Goals = lazy(() => import("./Goals/Goals"));
const Trends = lazy(() => import("./Trends/Trends"));
const ExternalAccounts = lazy(
  () => import("./ExternalAccounts/ExternalAccounts"),
);
const Settings = lazy(() => import("./Settings/Settings"));
const SettingsUser = lazy(() => import("./Settings/SettingsUser/SettingsUser"));
const SettingsSecurity = lazy(
  () => import("./Settings/SettingsSecurity/SettingsSecurity"),
);
const SettingsAdvanced = lazy(
  () => import("./Settings/AdvancedSettings/AdvancedSettings"),
);

const PageContent = (): React.ReactNode => {
  const suspenseFallback = (
    <Stack
      h={"calc(100dvh - var(--app-shell-header-offset, 60px))"}
      justify="center"
      align="center"
    >
      <LoadingScreen fullScreen={false} />
    </Stack>
  );

  return (
    <ScrollArea
      w="100%"
      h="100%"
      type="hover"
      pb="0.3rem"
      offsetScrollbars="present"
    >
      <Stack w="100%" p="0.5rem" pb="var(--bulk-bar-height, 0)" align="center">
        <Suspense fallback={suspenseFallback}>
          <Routes>
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/accounts">
              <Route index element={<Accounts />} />
              <Route path="settings" element={<AccountsSettings />}>
                <Route
                  index
                  element={<Navigate to="account-types" replace />}
                />
                <Route
                  path="account-types"
                  element={<AccountsSettingsAccountTypes />}
                />
                <Route path="deleted" element={<AccountsSettingsDeleted />} />
              </Route>
            </Route>
            <Route path="/assets">
              <Route index element={<Assets />} />
              <Route path="settings" element={<AssetsSettings />}>
                <Route index element={<Navigate to="deleted" replace />} />
                <Route path="deleted" element={<AssetsSettingsDeleted />} />
              </Route>
            </Route>
            <Route path="/transactions">
              <Route index element={<Transactions />} />
              <Route path="settings" element={<TransactionsSettings />}>
                <Route index element={<Navigate to="categories" replace />} />
                <Route
                  path="categories"
                  element={<TransactionsSettingsCategories />}
                />
                <Route path="rules" element={<TransactionsSettingsRules />} />
                <Route
                  path="deleted"
                  element={<TransactionsSettingsDeleted />}
                />
                <Route
                  path="auto-categorizer"
                  element={<TransactionsSettingsAutoCategorizer />}
                />
              </Route>
            </Route>
            <Route path="/budgets">
              <Route index element={<Budgets />} />
              <Route path="settings" element={<BudgetsSettings />} />
            </Route>
            <Route path="/goals" element={<Goals />} />
            <Route path="/trends" element={<Trends />} />
            <Route path="/external-accounts" element={<ExternalAccounts />} />
            <Route path="/settings" element={<Settings />}>
              <Route index element={<Navigate to="user" replace />} />
              <Route path="user" element={<SettingsUser />} />
              <Route path="security" element={<SettingsSecurity />} />
              <Route path="advanced" element={<SettingsAdvanced />} />
            </Route>
          </Routes>
        </Suspense>
      </Stack>
    </ScrollArea>
  );
};

export default PageContent;
