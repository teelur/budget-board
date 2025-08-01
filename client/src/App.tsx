import "./App.css";
import "@mantine/core/styles.css";
import "@mantine/notifications/styles.css";
import "@mantine/notifications/styles.layer.css";
import "@mantine/dates/styles.css";
import "@mantine/charts/styles.css";

import { BrowserRouter, Route, Routes } from "react-router";
import {
  createTheme,
  CSSVariablesResolver,
  MantineProvider,
} from "@mantine/core";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Notifications } from "@mantine/notifications";

import Welcome from "~/app/Unauthorized/Welcome";
import AuthProvider from "~/components/AuthProvider/AuthProvider";
import AuthorizedRoute from "~/components/AuthProvider/AuthorizedRoute";
import UnauthorizedRoute from "~/components/AuthProvider/UnauthorizedRoute";
import Authorized from "~/app/Authorized/Authorized";

// Your theme configuration is merged with default theme
const theme = createTheme({
  defaultRadius: "xs",
  primaryColor: "indigo",
  breakpoints: {
    xs: "30em", // 480px
    sm: "48em", // 768px
    md: "62em", // 992px
    lg: "75em", // 1200px
    xl: "90em", // 1440px
    transactionBreakpoint: "750px", // Custom breakpoint for transactions
    editTransactionBreakpoint: "850px", // Custom breakpoint for edit transactions
  },
});

const resolver: CSSVariablesResolver = (theme) => ({
  variables: {},
  light: {
    "--mantine-color-header-background": theme.colors.gray[0],
    "--mantine-color-content-background": theme.colors.gray[1],
    "--mantine-color-sidebar-background": theme.colors.gray[4],
    "--mantine-color-card-alternate": theme.colors.gray[2],
  },
  dark: {
    "--mantine-color-header-background": theme.colors.dark[8],
    "--mantine-color-content-background": theme.colors.dark[7],
    "--mantine-color-sidebar-background": theme.colors.dark[6],
    "--mantine-color-card-alternate": theme.colors.dark[7],
  },
});

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30000,
    },
  },
});

function App() {
  return (
    <MantineProvider
      theme={theme}
      cssVariablesResolver={resolver}
      defaultColorScheme="dark"
    >
      <AuthProvider>
        <QueryClientProvider client={queryClient}>
          <Notifications />
          <BrowserRouter>
            <Routes>
              <Route
                path="/"
                element={
                  <UnauthorizedRoute>
                    <Welcome />
                  </UnauthorizedRoute>
                }
              />
              <Route
                path="/dashboard"
                element={
                  <AuthorizedRoute>
                    <Authorized />
                  </AuthorizedRoute>
                }
              />
            </Routes>
          </BrowserRouter>
          {/* <ReactQueryDevtools initialIsOpen={false} /> */}
        </QueryClientProvider>
      </AuthProvider>
    </MantineProvider>
  );
}

export default App;
