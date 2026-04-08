import { ScrollArea, Stack } from "@mantine/core";
import { Route, Routes } from "react-router";
import { Suspense, lazy } from "react";
import LoadingScreen from "~/components/LoadingScreen/LoadingScreen";

const Dashboard = lazy(() => import("./Dashboard/Dashboard"));
const Accounts = lazy(() => import("./Accounts/Accounts"));
const Assets = lazy(() => import("./Assets/Assets"));
const Transactions = lazy(() => import("./Transactions/Transactions"));
const Budgets = lazy(() => import("./Budgets/Budgets"));
const Goals = lazy(() => import("./Goals/Goals"));
const Trends = lazy(() => import("./Trends/Trends"));
const ExternalAccounts = lazy(
  () => import("./ExternalAccounts/ExternalAccounts"),
);
const Settings = lazy(() => import("./Settings/Settings"));

const PageContent = (): React.ReactNode => {
  return (
    <ScrollArea
      style={{ width: "100%", height: "100%" }}
      type="auto"
      offsetScrollbars="present"
    >
      <Stack
        align="center"
        justify="flex-start"
        w="100%"
        h="100%"
        flex="1 1 auto"
        pb="var(--bulk-bar-height, 0)"
      >
        <Suspense fallback={<LoadingScreen />}>
          <Routes>
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/accounts" element={<Accounts />} />
            <Route path="/assets" element={<Assets />} />
            <Route path="/transactions" element={<Transactions />} />
            <Route path="/budgets" element={<Budgets />} />
            <Route path="/goals" element={<Goals />} />
            <Route path="/trends" element={<Trends />} />
            <Route path="/external-accounts" element={<ExternalAccounts />} />
            <Route path="/settings" element={<Settings />} />
          </Routes>
        </Suspense>
      </Stack>
    </ScrollArea>
  );
};

export default PageContent;
