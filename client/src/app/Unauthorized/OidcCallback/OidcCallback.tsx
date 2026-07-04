import React from "react";
import { notifications } from "@mantine/notifications";
import { AxiosError, AxiosResponse } from "axios";
import {
  applicationUserQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import {
  AuthContext,
  AuthContextValue,
} from "~/providers/AuthProvider/AuthProvider";
import { useNavigate } from "react-router";
import {
  IOidcCallbackRequest,
  IOidcCallbackResponse,
  IOidcConnectRequest,
  OidcAuthFlow,
  OidcAuthFlows,
} from "~/models/oidc";
import LoadingScreen from "~/components/LoadingScreen/LoadingScreen";
import { useTranslation } from "react-i18next";
import { useQueryClient } from "@tanstack/react-query";

const OidcCallback = (): React.ReactNode => {
  const { t } = useTranslation();

  const { request, setIsUserAuthenticated } =
    React.useContext<AuthContextValue>(AuthContext);
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const hasProcessed = React.useRef(false);

  React.useEffect(() => {
    if (hasProcessed.current) {
      return;
    }

    hasProcessed.current = true;

    (async () => {
      const q = new URLSearchParams(window.location.search);
      const code = q.get("code");
      const state = q.get("state");
      const error = q.get("error");
      const errorDescription = q.get("error_description");

      const savedState = state
        ? sessionStorage.getItem(`oidc_state_${state}`)
        : null;
      const flow = state
        ? (sessionStorage.getItem(`oidc_flow_${state}`) as OidcAuthFlow | null)
        : null;
      const oidcFlow = flow ?? OidcAuthFlows.SignIn;

      const clearOidcState = (stateValue: string | null): void => {
        if (!stateValue) {
          return;
        }

        sessionStorage.removeItem(`oidc_state_${stateValue}`);
        sessionStorage.removeItem(`oidc_remember_me_${stateValue}`);
        sessionStorage.removeItem(`oidc_flow_${stateValue}`);
      };

      if (error) {
        clearOidcState(state);
        notifications.show({
          color: "var(--button-color-destructive)",
          message: `Authentication failed: ${errorDescription || error}`,
        });
        navigate(
          oidcFlow === OidcAuthFlows.Connect ? "/settings/security" : "/",
        );
        return;
      }

      if (!code) {
        clearOidcState(state);
        notifications.show({
          color: "var(--button-color-destructive)",
          message: t("authorization_code_missing_message"),
        });
        navigate(
          oidcFlow === OidcAuthFlows.Connect ? "/settings/security" : "/",
        );
        return;
      }

      if (!state || state !== savedState) {
        clearOidcState(state);
        notifications.show({
          color: "var(--button-color-destructive)",
          message: t("state_parameter_invalid_message"),
        });
        navigate("/");
        return;
      }

      try {
        const rememberMe =
          sessionStorage.getItem(`oidc_remember_me_${state}`) === "true";

        if (oidcFlow === OidcAuthFlows.Connect) {
          await request({
            url: "/api/applicationuser/connectoidclogin",
            method: "POST",
            data: {
              code,
              redirect_uri: `${window.location.origin}/oidc-callback`,
            } as IOidcConnectRequest,
          });

          await queryClient.invalidateQueries({
            queryKey: [applicationUserQueryKey],
          });

          clearOidcState(state);
          navigate("/settings/security");
          return;
        }

        const response: AxiosResponse<IOidcCallbackResponse> = await request({
          url: "/api/oidc/callback",
          method: "POST",
          data: {
            code,
            redirect_uri: `${window.location.origin}/oidc-callback`,
            remember_me: rememberMe,
          } as IOidcCallbackRequest,
        });

        clearOidcState(state);

        setIsUserAuthenticated(response.data?.success ?? false);

        if (!response.data?.success) {
          notifications.show({
            color: "var(--button-color-destructive)",
            message: t("oidc_authentication_failed_message"),
          });
          navigate("/");
          return;
        }

        navigate("/dashboard");
      } catch (e) {
        clearOidcState(state);
        const err = e as AxiosError;
        notifications.show({
          color: "var(--button-color-destructive)",
          message: translateAxiosError(err),
        });
        navigate(
          oidcFlow === OidcAuthFlows.Connect ? "/settings/security" : "/",
        );
      }
    })();
  }, []);

  return <LoadingScreen />;
};

export default OidcCallback;
