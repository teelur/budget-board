import React from "react";
import { notifications } from "@mantine/notifications";
import { AxiosError, AxiosResponse } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import {
  AuthContext,
  AuthContextValue,
} from "~/providers/AuthProvider/AuthProvider";
import { useNavigate } from "react-router";
import { IOidcCallbackRequest, IOidcCallbackResponse } from "~/models/oidc";
import LoadingScreen from "~/components/LoadingScreen/LoadingScreen";
import { useTranslation } from "react-i18next";

const OidcCallback = (): React.ReactNode => {
  const { t } = useTranslation();

  const { request, setIsUserAuthenticated } =
    React.useContext<AuthContextValue>(AuthContext);
  const navigate = useNavigate();

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

      if (error) {
        if (savedState) {
          sessionStorage.removeItem(`oidc_state_${state}`);
        }
        notifications.show({
          color: "var(--button-color-destructive)",
          message: `Authentication failed: ${errorDescription || error}`,
        });
        navigate("/");
        return;
      }

      if (!code) {
        if (savedState) {
          sessionStorage.removeItem(`oidc_state_${state}`);
        }
        notifications.show({
          color: "var(--button-color-destructive)",
          message: t("authorization_code_missing_message"),
        });
        navigate("/");
        return;
      }

      if (!state || state !== savedState) {
        if (savedState) {
          sessionStorage.removeItem(`oidc_state_${state}`);
        }
        notifications.show({
          color: "var(--button-color-destructive)",
          message: t("state_parameter_invalid_message"),
        });
        navigate("/");
        return;
      }

      try {
        const response: AxiosResponse<IOidcCallbackResponse> = await request({
          url: "/api/oidc/callback",
          method: "POST",
          data: {
            code,
            redirect_uri: `${window.location.origin}/oidc-callback`,
          } as IOidcCallbackRequest,
        });

        if (savedState) {
          sessionStorage.removeItem(`oidc_state_${state}`);
        }

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
        if (savedState) {
          sessionStorage.removeItem(`oidc_state_${state}`);
        }
        const err = e as AxiosError;
        notifications.show({
          color: "var(--button-color-destructive)",
          message: translateAxiosError(err),
        });
        navigate("/");
      }
    })();
  }, []);

  return <LoadingScreen />;
};

export default OidcCallback;
