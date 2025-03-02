import "./App.css";
import "@mantine/core/styles.css";
import "@mantine/notifications/styles.css";
import "@mantine/notifications/styles.layer.css";
import "@mantine/dates/styles.css";

import { BrowserRouter, Route, Routes } from "react-router";
import { Center, createTheme, MantineProvider } from "@mantine/core";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Notifications } from "@mantine/notifications";

import Welcome from "./app/unauthorized/Welcome";
import AuthProvider from "./components/auth/AuthProvider";
import AuthorizedRoute from "./components/auth/AuthorizedRoute";
import UnauthorizedRoute from "./components/auth/UnauthorizedRoute";
import Authorized from "./app/authorized/Authorized";

// Your theme configuration is merged with default theme
const theme = createTheme({
  defaultRadius: "xs",
  primaryColor: "indigo",
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
    <MantineProvider theme={theme} defaultColorScheme="light">
      <AuthProvider>
        <QueryClientProvider client={queryClient}>
          <Notifications />
          <Center>
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
          </Center>
        </QueryClientProvider>
      </AuthProvider>
    </MantineProvider>
  );
}

export default App;
