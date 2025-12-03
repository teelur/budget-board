import { getProjectEnvVariables } from "~/shared/projectEnvVariables";
import axios, { AxiosError, AxiosResponse } from "axios";
import React, { createContext, useState } from "react";
import { notifications } from "@mantine/notifications";
import { IOidcDiscoveryDocument } from "~/models/oidc";

export interface AuthContextValue {
  isUserAuthenticated: boolean;
  setIsUserAuthenticated: (isLoggedIn: boolean) => void;
  loading: boolean;
  request: ({ ...options }) => Promise<AxiosResponse>;
  startOidcLogin?: () => void;
  oidcLoading: boolean;
}

export const AuthContext = createContext<AuthContextValue>({
  isUserAuthenticated: false,
  setIsUserAuthenticated: () => {},
  loading: false,
  request: async () => {
    return {} as AxiosResponse;
  },
  startOidcLogin: () => {},
  oidcLoading: false,
});

export const AuthProvider = ({
  children,
}: {
  children: React.ReactNode;
}): React.ReactNode => {
  const [isUserAuthenticated, setIsUserAuthenticated] =
    useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);
  const [oidcLoading, setOidcLoading] = useState<boolean>(false);

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
        setIsUserAuthenticated(res.data?.isAuthenticated ?? false);
      })
      .catch(() => {
        notifications.show({
          message: "Failed to check authentication status",
          color: "red",
        });
        setIsUserAuthenticated(false);
      })
      .finally(() => {
        setLoading(false);
      });
  }, []);

  const startOidcLogin = async (): Promise<void> => {
    setOidcLoading(true);
    try {
      let authorizeUrl = envVariables.VITE_OIDC_PROVIDER;
      if (authorizeUrl) {
        authorizeUrl = authorizeUrl.replace(/\/+$/, "");
      }
      const clientId = envVariables.VITE_OIDC_CLIENT_ID;
      const redirectUri = `${window.location.origin}/oidc-callback`;

      if (!authorizeUrl || !clientId) {
        notifications.show({
          color: "red",
          message:
            "OIDC is enabled but not configured. Be sure all required environment variables are set.",
        });
        return;
      }

      const state = crypto.randomUUID();
      sessionStorage.setItem(`oidc_state_${state}`, state);

      const params = new URLSearchParams({
        client_id: clientId,
        redirect_uri: redirectUri,
        response_type: "code",
        scope: "openid profile email",
        state,
      });

      const discoveryUrl = `${authorizeUrl}/.well-known/openid-configuration`;

      const discoveryResponse = await fetch(discoveryUrl);
      if (!discoveryResponse.ok) {
        throw new Error("Failed to fetch OIDC discovery document");
      }

      const discoveryData: IOidcDiscoveryDocument =
        await discoveryResponse.json();

      if (discoveryData?.authorization_endpoint) {
        window.location.href = `${
          discoveryData.authorization_endpoint
        }?${params.toString()}`;
      }
    } catch (error) {
      notifications.show({
        color: "red",
        message: "Failed to retrieve OIDC discovery document.",
      });
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
