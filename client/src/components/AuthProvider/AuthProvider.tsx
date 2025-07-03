import { getProjectEnvVariables } from "~/shared/projectEnvVariables";
import axios, { AxiosError, AxiosResponse } from "axios";
import React, { createContext, useState } from "react";
import { notifications } from "@mantine/notifications";

export const AuthContext = createContext({});

export interface AuthContextValue {
  isUserAuthenticated: boolean;
  setIsUserAuthenticated: (isLoggedIn: boolean) => void;
  loading: boolean;
  request: ({ ...options }) => Promise<AxiosResponse>;
}

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

  const authValue: AuthContextValue = {
    isUserAuthenticated,
    setIsUserAuthenticated,
    loading,
    request,
  };

  return (
    <AuthContext.Provider value={authValue}>{children}</AuthContext.Provider>
  );
};

export default AuthProvider;
