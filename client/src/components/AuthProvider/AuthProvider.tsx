import { getProjectEnvVariables } from "~/shared/projectEnvVariables";
import axios, { AxiosError, AxiosResponse } from "axios";
import React, { createContext, useState } from "react";
import { notifications } from "@mantine/notifications";

export interface AuthContextValue {
  isUserAuthenticated: boolean;
  setIsUserAuthenticated: (isLoggedIn: boolean) => void;
  loading: boolean;
  request: ({ ...options }) => Promise<AxiosResponse>;
  startOidcLogin?: () => void; // added
}

export const AuthContext = createContext<AuthContextValue>({
  isUserAuthenticated: false,
  setIsUserAuthenticated: () => {},
  loading: false,
  request: async () => {
    return {} as AxiosResponse;
  },
});

const AuthProvider = ({ children }: { children: any }): React.ReactNode => {
  const [isUserAuthenticated, setIsUserAuthenticated] =
    useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);

  const { envVariables } = getProjectEnvVariables();

  // base url is sourced from environment variables
  const client = axios.create({
    baseURL: envVariables.VITE_API_URL,
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
      })
      .finally(() => {
        setLoading(false);
      });
  }, []);

  const startOidcLogin = async (): Promise<void> => {
    const authorizeUrl = envVariables.VITE_OIDC_PROVIDER;
    const clientId = envVariables.VITE_OIDC_CLIENT_ID;
    const redirectUri = `${window.location.origin}/oidc-callback`;

    if (!authorizeUrl || !clientId) {
      notifications.show({
        color: "red",
        message:
          "OIDC is not configured. Set VITE_OIDC_PROVIDER and VITE_OIDC_CLIENT_ID.",
      });
      return;
    }

    const state = Math.random().toString(36).slice(2);
    sessionStorage.setItem("oidc_state", state);

    const params = new URLSearchParams({
      client_id: clientId,
      redirect_uri: redirectUri,
      response_type: "code",
      scope: "openid profile email",
      state,
    });

    window.location.href = `${authorizeUrl}/authorize?${params.toString()}`;
  };

  const authValue: AuthContextValue = {
    isUserAuthenticated,
    setIsUserAuthenticated,
    loading,
    request,
    startOidcLogin,
  };

  return (
    <AuthContext.Provider value={authValue}>{children}</AuthContext.Provider>
  );
};

export default AuthProvider;
