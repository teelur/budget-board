import { ScrollArea, Stack } from "@mantine/core";
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
const AssetsSettings = lazy(
  () => import("./Assets/AssetsSettings/AssetsSettings"),
);
const AssetsSettingsDeleted = lazy(
  () => import("./Assets/AssetsSettings/DeletedAssets/DeletedAssets"),
);
const TransactionsSettingsCategories = lazy(
  () =>
    import("./Transactions/TransactionsSettings/CustomCategories/CustomCategories"),
);
const TransactionsSettingsRules = lazy(
  () =>
    import("./Transactions/TransactionsSettings/AutomaticRules/AutomaticRules"),
);
const TransactionsSettingsDeleted = lazy(
  () =>
    import("./Transactions/TransactionsSettings/DeletedTransactions/DeletedTransactions"),
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
  return (
    <ScrollArea
      style={{
        width: "100%",
        height: "100%",
      }}
      type="auto"
      offsetScrollbars="present"
    >
      <Stack
        align="center"
        justify="flex-start"
        w="100%"
        h="100%"
        flex="1 1 auto"
        p="0.5rem"
        pb="var(--bulk-bar-height, 0)"
      >
        <Suspense fallback={<LoadingScreen />}>
          <Routes>
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/accounts">
              <Route index element={<Accounts />} />
              <Route path="settings" element={<AccountsSettings />}>
                <Route index element={<Navigate to="deleted" replace />} />
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
