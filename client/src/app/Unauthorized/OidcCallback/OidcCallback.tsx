import React from "react";
import { notifications } from "@mantine/notifications";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import {
  AuthContext,
  AuthContextValue,
} from "~/providers/AuthProvider/AuthProvider";
import { useNavigate } from "react-router";
import {
  IOidcCallbackRequest,
  IOidcConnectRequest,
  OidcAuthFlowFailedRedirectEndpoints,
  OidcAuthFlowSuccessRedirectEndpoints,
  OidcAuthFlows,
} from "~/models/oidc";
import LoadingScreen from "~/components/LoadingScreen/LoadingScreen";
import { useTranslation } from "react-i18next";
import { useConnectOidcLoginMutation } from "~/hooks/mutations/applicationUser/useConnectOidcLoginMutation";
import { useOidcCallbackMutation } from "~/hooks/mutations/oidc/useOidcCallbackMutation";

const OidcCallback = (): React.ReactNode => {
  const { t } = useTranslation();
  const connectOidcLoginMutation = useConnectOidcLoginMutation();
  const oidcCallbackMutation = useOidcCallbackMutation();

  const { setIsUserAuthenticated } =
    React.useContext<AuthContextValue>(AuthContext);
  const navigate = useNavigate();

  const hasProcessed = React.useRef(false);

  const clearOidcState = (stateValue: string | null): void => {
    if (!stateValue) {
      return;
    }

    sessionStorage.removeItem(`oidc_state_${stateValue}`);
    sessionStorage.removeItem(`oidc_remember_me_${stateValue}`);
    sessionStorage.removeItem(`oidc_flow_${stateValue}`);
  };

  const handleOidcCallback = async () => {
    // The OIDC provider will redirect the user back to this callback URL with query parameters.
    const q = new URLSearchParams(window.location.search);
    const code = q.get("code");
    const state = q.get("state");
    const error = q.get("error");
    const errorDescription = q.get("error_description");

    // We stored the state and flow in sessionStorage before redirecting to the OIDC provider.
    const savedState = state
      ? sessionStorage.getItem(`oidc_state_${state}`)
      : null;
    const rawFlow = state ? sessionStorage.getItem(`oidc_flow_${state}`) : null;
    const oidcFlow =
      rawFlow === OidcAuthFlows.Connect || rawFlow === OidcAuthFlows.SignIn
        ? rawFlow
        : OidcAuthFlows.SignIn;

    // The OIDC provider provided an error
    if (error) {
      clearOidcState(state);
      notifications.show({
        color: "var(--button-color-destructive)",
        message: t("oidc_provider_error_message", {
          error: errorDescription ?? error,
        }),
      });
      navigate(OidcAuthFlowFailedRedirectEndpoints[oidcFlow]);
      return;
    }

    // An authorization code is required to complete the OIDC flow.
    if (!code) {
      clearOidcState(state);
      notifications.show({
        color: "var(--button-color-destructive)",
        message: t("authorization_code_missing_message"),
      });
      navigate(OidcAuthFlowFailedRedirectEndpoints[oidcFlow]);
      return;
    }

    // The state parameter is required to prevent CSRF attacks.
    if (!state || state !== savedState) {
      clearOidcState(state);
      notifications.show({
        color: "var(--button-color-destructive)",
        message: t("state_parameter_invalid_message"),
      });
      navigate(OidcAuthFlowFailedRedirectEndpoints[oidcFlow]);
      return;
    }

    try {
      if (oidcFlow === OidcAuthFlows.Connect) {
        await connectOidcLoginMutation.mutateAsync(
          {
            code,
            redirect_uri: `${window.location.origin}/oidc-callback`,
          } as IOidcConnectRequest,
          {
            onSuccess: () => {
              clearOidcState(state);
            },
          },
        );
        navigate(OidcAuthFlowSuccessRedirectEndpoints[oidcFlow]);
        return;
      } else if (oidcFlow == OidcAuthFlows.SignIn) {
        const rememberMe =
          sessionStorage.getItem(`oidc_remember_me_${state}`) === "true";
        const response = await oidcCallbackMutation.mutateAsync({
          code,
          redirect_uri: `${window.location.origin}/oidc-callback`,
          remember_me: rememberMe,
        } as IOidcCallbackRequest);
        clearOidcState(state);
        setIsUserAuthenticated(response.data?.success ?? false);
        if (response.data?.success) {
          navigate(OidcAuthFlowSuccessRedirectEndpoints[oidcFlow]);
        } else {
          notifications.show({
            color: "var(--button-color-destructive)",
            message: t("oidc_authentication_failed_message"),
          });
          navigate(OidcAuthFlowFailedRedirectEndpoints[oidcFlow]);
        }
      }
    } catch (error) {
      clearOidcState(state);
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error as AxiosError),
      });
      navigate(OidcAuthFlowFailedRedirectEndpoints[oidcFlow]);
    }
  };

  React.useEffect(() => {
    if (hasProcessed.current) {
      return;
    }
    hasProcessed.current = true;
    handleOidcCallback();
  }, []);

  return <LoadingScreen />;
};

export default OidcCallback;
