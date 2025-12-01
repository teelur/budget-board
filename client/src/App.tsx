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
import OidcCallback from "~/app/Unauthorized/OidcCallback";
import { AuthProvider } from "~/providers/AuthProvider/AuthProvider";
import AuthorizedRoute from "~/providers/AuthProvider/AuthorizedRoute";
import UnauthorizedRoute from "~/providers/AuthProvider/UnauthorizedRoute";
import Authorized from "~/app/Authorized/Authorized";
import {
  backgroundGray,
  backgroundWhite,
  textBlack,
  textGray,
  green,
  blue,
  orange,
  red,
} from "./shared/colors";

// Your theme configuration is merged with default theme
const theme = createTheme({
  cursorType: "pointer",
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
  colors: {
    backgroundWhite,
    backgroundGray,
    textGray,
  },
});

const resolver: CSSVariablesResolver = (theme) => ({
  variables: {},
  light: {
    // Base colors
    "--background-color-base": backgroundWhite[0],
    "--base-color-border": backgroundWhite[4],
    "--base-color-text-primary": textBlack[1],
    "--base-color-text-dimmed": textBlack[7],
    "--base-color-input-background": backgroundWhite[2],
    "--base-color-input-border": backgroundWhite[6],
    // Sidebar and Header colors
    "--background-color-sidebar": backgroundWhite[2],
    "--background-color-header": backgroundWhite[1],
    // Surface colors
    "--background-color-surface": backgroundWhite[1],
    "--surface-color-border": backgroundWhite[3],
    "--surface-color-text-primary": textBlack[1],
    "--surface-color-text-dimmed": textBlack[6],
    "--surface-color-input-background": backgroundWhite[2],
    "--surface-color-input-border": backgroundWhite[5],
    // Elevated colors
    "--background-color-elevated": backgroundWhite[2],
    "--elevated-color-border": backgroundWhite[5],
    "--elevated-color-text-primary": textBlack[1],
    "--elevated-color-text-dimmed": textBlack[7],
    "--elevated-color-input-background": backgroundWhite[3],
    "--elevated-color-input-border": backgroundWhite[7],
    // Text Status colors
    "--text-color-status-good": green[9],
    "--text-color-status-neutral": blue[7],
    "--text-color-status-warning": orange[6],
    "--text-color-status-bad": red[5],
    // Button colors
    "--button-color-confirm": green[9],
    "--button-color-destructive": red[5],

    // TODO: remove
    "--mantine-color-text": theme.colors.dark[7],
    "--mantine-color-header-background": theme.colors.gray[0],
    "--mantine-color-content-background": theme.colors.gray[1],
    "--mantine-color-sidebar-background": theme.colors.gray[4],
    "--mantine-color-card-alternate": theme.colors.gray[2],
    "--mantine-color-light-off": theme.colors.gray[4],
    "--mantine-color-accordion-alternate": theme.colors.gray[3],
  },
  dark: {
    // Base colors
    "--background-color-base": backgroundGray[9],
    "--base-color-border": backgroundGray[4],
    "--base-color-text-primary": textGray[0],
    "--base-color-text-dimmed": textGray[8],
    "--base-color-input-background": backgroundGray[5],
    "--base-color-input-border": backgroundGray[1],
    // Sidebar and Header colors
    "--background-color-sidebar": backgroundGray[7],
    "--background-color-header": backgroundGray[8],
    // Surface colors
    "--background-color-surface": backgroundGray[7],
    "--surface-color-border": backgroundGray[3],
    "--surface-color-text-primary": textGray[0],
    "--surface-color-text-dimmed": textGray[9],
    "--surface-color-input-background": backgroundGray[5],
    "--surface-color-input-border": backgroundGray[2],
    // Elevated colors
    "--background-color-elevated": backgroundGray[5],
    "--elevated-color-border": backgroundGray[0],
    "--elevated-color-text-primary": textGray[0],
    "--elevated-color-text-dimmed": textGray[9],
    "--elevated-color-input-background": backgroundGray[6],
    "--elevated-color-input-border": backgroundGray[2],
    // Text Status colors
    "--text-color-status-good": green[8],
    "--text-color-status-neutral": blue[5],
    "--text-color-status-warning": orange[5],
    "--text-color-status-bad": red[4],
    // Button colors
    "--button-color-confirm": green[9],
    "--button-color-destructive": red[4],
    // TODO: remove
    "--mantine-color-header-background": theme.colors.dark[8],
    "--mantine-color-content-background": theme.colors.dark[7],
    "--mantine-color-sidebar-background": theme.colors.dark[6],
    "--mantine-color-card-alternate": theme.colors.dark[7],
    "--mantine-color-light-off": theme.colors.dark[4],
    "--mantine-color-accordion-alternate": theme.colors.dark[8],
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
              <Route path="/oidc-callback" element={<OidcCallback />} />
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
