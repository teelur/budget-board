import { getProjectEnvVariables } from "~/shared/projectEnvVariables";
import axios, { AxiosError, AxiosResponse } from "axios";
import React, { createContext, useState } from "react";
import { notifications } from "@mantine/notifications";
import {
  IOidcDiscoveryDocument,
  OidcAuthFlow,
  OidcAuthFlows,
} from "~/models/oidc";
import { useTranslation } from "react-i18next";

export interface AuthContextValue {
  isUserAuthenticated: boolean;
  setIsUserAuthenticated: (isLoggedIn: boolean) => void;
  loading: boolean;
  request: ({ ...options }) => Promise<AxiosResponse>;
  startOidcLogin?: (rememberMe: boolean, flow?: OidcAuthFlow) => void;
  oidcLoading: boolean;
}

export const AuthContext = createContext<AuthContextValue>({
  isUserAuthenticated: false,
  setIsUserAuthenticated: () => {},
  loading: false,
  request: async () => {
    return {} as AxiosResponse;
  },
  startOidcLogin: undefined,
  oidcLoading: false,
});

export const AuthProvider = ({
  children,
}: {
  children: React.ReactNode;
}): React.ReactNode => {
  const cachedAuth = localStorage.getItem("isAuthenticated") === "true";
  const [isUserAuthenticated, setIsUserAuthenticated] =
    useState<boolean>(cachedAuth);
  const [loading, setLoading] = useState<boolean>(true);
  const [oidcLoading, setOidcLoading] = useState<boolean>(false);

  const { t } = useTranslation();
  const { envVariables } = getProjectEnvVariables();

  // base url is sourced from environment variables
  const client = axios.create({
    baseURL: import.meta.env.DEV
      ? `http://${envVariables.VITE_BUDGET_BOARD_DOMAIN}`
      : undefined,
    withCredentials: true,
  });

  const request = async ({ ...options }): Promise<AxiosResponse> => {
    const onSuccess = (response: AxiosResponse): AxiosResponse => response;
    const onError = (error: AxiosError): any => {
      if (isUserAuthenticated && error.response?.status === 401) {
        notifications.show({
          message: t("unauthorized_message"),
          color: "var(--button-color-destructive)",
        });

        localStorage.setItem("isAuthenticated", "false");
        setIsUserAuthenticated(false);
      }

      throw error;
    };

    return await client(options).then(onSuccess).catch(onError);
  };

  React.useEffect(() => {
    setLoading(true);

    request({
      url: "/api/isAuthenticated",
      method: "GET",
    })
      .then((res: AxiosResponse) => {
        const authed = res.data?.isAuthenticated ?? false;
        localStorage.setItem("isAuthenticated", String(authed));
        setIsUserAuthenticated(authed);
      })
      .catch(() => {
        notifications.show({
          message: "Failed to check authentication status",
          color: "var(--button-color-destructive)",
        });
        localStorage.setItem("isAuthenticated", "false");
        setIsUserAuthenticated(false);
      })
      .finally(() => {
        setLoading(false);
      });
  }, []);

  const startOidcLogin = async (
    rememberMe: boolean,
    flow: OidcAuthFlow = OidcAuthFlows.SignIn,
  ): Promise<void> => {
    setOidcLoading(true);
    try {
      let authorizeUrl = envVariables.VITE_OIDC_PROVIDER;
      // Trailing slashes can cause issues with the discovery document, so
      // we need to remove them if present
      if (authorizeUrl) {
        authorizeUrl = authorizeUrl.replace(/\/+$/, "");
      }
      const clientId = envVariables.VITE_OIDC_CLIENT_ID;
      const redirectUri = `${window.location.origin}/oidc-callback`;

      if (!authorizeUrl || !clientId) {
        notifications.show({
          color: "var(--button-color-destructive)",
          message: t("oidc_enabled_but_not_configured"),
        });
        return;
      }

      // These are stored in sessionStorage to be retrieved after the OIDC callback
      const state = crypto.randomUUID();
      sessionStorage.setItem(`oidc_state_${state}`, state);
      sessionStorage.setItem(`oidc_remember_me_${state}`, String(rememberMe));
      sessionStorage.setItem(`oidc_flow_${state}`, flow);

      const params = new URLSearchParams({
        client_id: clientId,
        redirect_uri: redirectUri,
        response_type: "code",
        scope: "openid profile email",
        state,
      });

      // The discovery document is used to retrieve the authorization endpoint,
      // which may vary between OIDC providers.
      const discoveryUrl = `${authorizeUrl}/.well-known/openid-configuration`;

      const discoveryResponse = await fetch(discoveryUrl);
      if (!discoveryResponse.ok) {
        notifications.show({
          color: "var(--button-color-destructive)",
          message: t("oidc_discovery_document_failed_message"),
        });
        return;
      }

      const discoveryData: IOidcDiscoveryDocument =
        await discoveryResponse.json();

      // This redirects the user to the OIDC provider's login page.
      if (discoveryData?.authorization_endpoint) {
        window.location.href = `${
          discoveryData.authorization_endpoint
        }?${params.toString()}`;
      } else {
        notifications.show({
          color: "var(--button-color-destructive)",
          message: t("oidc_redirect_failed_message"),
        });
      }
    } catch (error) {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: t("oidc_redirect_unspecified_error_message"),
      });
    } finally {
      setOidcLoading(false);
    }
  };

  const authValue: AuthContextValue = {
    isUserAuthenticated,
    setIsUserAuthenticated,
    loading,
    request,
    startOidcLogin,
    oidcLoading,
  };

  return (
    <AuthContext.Provider value={authValue}>{children}</AuthContext.Provider>
  );
};

export const useAuth = () => React.useContext(AuthContext);
