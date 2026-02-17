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
import OidcCallback from "~/app/Unauthorized/OidcCallback/OidcCallback";
import { AuthProvider } from "~/providers/AuthProvider/AuthProvider";
import AuthorizedRoute from "~/providers/AuthProvider/AuthorizedRoute";
import UnauthorizedRoute from "~/providers/AuthProvider/UnauthorizedRoute";
import Authorized from "~/app/Authorized/Authorized";
import {
  backgroundGray,
  backgroundEggshell,
  textBlack,
  textGray,
  green,
  blue,
  orange,
  red,
  borderEggshell,
  textEggshellDimmed,
} from "./shared/colors";
import { UserSettingsProvider } from "./providers/UserSettingsProvider/UserSettingsProvider";
import { LocaleProvider } from "./providers/LocaleProvider/LocaleProvider";

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
    backgroundEggshell,
    borderEggshell,
    textEggshellDimmed,
    backgroundGray,
    textGray,
    textBlack,
    green,
    blue,
    orange,
    red,
  },
});

const resolver: CSSVariablesResolver = () => ({
  variables: {},
  light: {
    // Base colors
    "--background-color-base": backgroundEggshell[0],
    "--base-color-border": backgroundEggshell[4],
    "--base-color-text-primary": textBlack[1],
    "--base-color-text-dimmed": textEggshellDimmed[6],
    "--base-color-input-background": backgroundEggshell[2],
    "--base-color-input-border": backgroundEggshell[5],
    "--base-color-progress": backgroundEggshell[4],
    // Sidebar and Header colors
    "--background-color-sidebar": backgroundEggshell[2],
    "--background-color-header": backgroundEggshell[1],
    // Surface colors
    "--background-color-surface": backgroundEggshell[2],
    "--surface-color-border": borderEggshell[2],
    "--surface-color-text-primary": textBlack[1],
    "--surface-color-text-dimmed": textEggshellDimmed[6],
    "--surface-color-input-background": backgroundEggshell[5],
    "--surface-color-input-border": backgroundEggshell[8],
    "--surface-color-progress": backgroundEggshell[5],
    // Elevated colors
    "--background-color-elevated": backgroundEggshell[4],
    "--elevated-color-border": borderEggshell[3],
    "--elevated-color-text-primary": textBlack[1],
    "--elevated-color-text-dimmed": textEggshellDimmed[7],
    "--elevated-color-input-background": backgroundEggshell[6],
    "--elevated-color-input-border": backgroundEggshell[9],
    "--elevated-color-progress": backgroundEggshell[7],
    // Text Status colors
    "--text-color-status-good": green[9],
    "--text-color-status-neutral": blue[7],
    "--text-color-status-warning": orange[6],
    "--text-color-status-bad": red[5],
    // Button colors
    "--button-color-confirm": green[9],
    "--button-color-warning": orange[6],
    "--button-color-destructive": red[5],
    // Other
    "--light-color-off": backgroundEggshell[7],
  },
  dark: {
    // Base colors
    "--background-color-base": backgroundGray[9],
    "--base-color-border": backgroundGray[4],
    "--base-color-text-primary": textGray[0],
    "--base-color-text-dimmed": textGray[8],
    "--base-color-input-background": backgroundGray[7],
    "--base-color-input-border": backgroundGray[4],
    "--base-color-progress": backgroundGray[6],
    // Sidebar and Header colors
    "--background-color-sidebar": backgroundGray[7],
    "--background-color-header": backgroundGray[8],
    // Surface colors
    "--background-color-surface": backgroundGray[7],
    "--surface-color-border": backgroundGray[3],
    "--surface-color-text-primary": textGray[0],
    "--surface-color-text-dimmed": textGray[9],
    "--surface-color-input-background": backgroundGray[5],
    "--surface-color-input-border": backgroundGray[3],
    "--surface-color-progress": backgroundGray[4],
    // Elevated colors
    "--background-color-elevated": backgroundGray[5],
    "--elevated-color-border": backgroundGray[0],
    "--elevated-color-text-primary": textGray[0],
    "--elevated-color-text-dimmed": textGray[9],
    "--elevated-color-input-background": backgroundGray[3],
    "--elevated-color-input-border": backgroundGray[0],
    "--elevated-color-progress": backgroundGray[2],
    // Text Status colors
    "--text-color-status-good": green[8],
    "--text-color-status-neutral": blue[5],
    "--text-color-status-warning": orange[5],
    "--text-color-status-bad": red[4],
    // Button colors
    "--button-color-confirm": green[9],
    "--button-color-warning": orange[5],
    "--button-color-destructive": red[4],
    // Other
    "--light-color-off": backgroundGray[3],
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
                path="/oidc-callback"
                element={
                  <UnauthorizedRoute>
                    <OidcCallback />
                  </UnauthorizedRoute>
                }
              />
              <Route
                path="/dashboard"
                element={
                  <AuthorizedRoute>
                    <UserSettingsProvider>
                      <LocaleProvider>
                        <Authorized />
                      </LocaleProvider>
                    </UserSettingsProvider>
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
